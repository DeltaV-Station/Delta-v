using System.Linq;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.Components;
using Content.Shared._DV.MetalDetector;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Contraband;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Implants.Components;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Storage;
using Content.Shared.Toggleable;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.MetalDetector;

/// <summary>
/// Systems related to the Metal Detector and how it functions.
/// </summary>
public sealed class MetalDetectorSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetalDetectorComponent, StepTriggeredOnEvent>(HandleStepOnTriggered);
        SubscribeLocalEvent<MetalDetectorComponent, StepTriggeredOffEvent>(HandleStepOffTriggered);
        SubscribeLocalEvent<MetalDetectorComponent, StepTriggerAttemptEvent>(HandleStepTriggerAttempt);
        SubscribeLocalEvent<MetalDetectorComponent, GotEmaggedEvent>(OnEmagged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        using var query = EntityQueryEnumerator<MetalDetectorComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (!comp.IsSirenRunning || !TryComp<ItemToggleComponent>(ent, out var toggle) || !toggle.Activated)
                continue;

            var powered = TryComp<ApcPowerReceiverComponent>(ent, out var receiver) && receiver.Powered;

            TryComp<AppearanceComponent>(ent, out var appComp);

            if (comp.EndOfSirenSound <= _timing.CurTime || !powered)
            {
                comp.IsSirenRunning = false;
                var toggledEvent = new ItemToggledEvent(false, false, ent);
                toggle.Activated = false;
                _appearanceSystem.SetData(ent, MetalDetectorVisuals.MetalDetectorActivated, false);
                _appearanceSystem.SetData(ent, ToggleableVisuals.Enabled, false, appComp);
                RaiseLocalEvent(ent, ref toggledEvent);
            }
        }
    }

    private void HandleStepOnTriggered(Entity<MetalDetectorComponent> ent, ref StepTriggeredOnEvent args)
    {
        if (ent.Comp.IsSirenRunning)
        {
            ent.Comp.EndOfSirenSound = _timing.CurTime + ent.Comp.SirenRunTime;
            return;
        }

        var random = new Random();
        if (TryComp<ItemToggleComponent>(ent, out var toggleComponent) && (CheckForContraband(args.Tripper)
                || random.NextFloat(0.0f, 100.0f) < ent.Comp.FalsePositiveChance
                || HasComp<EmaggedComponent>(ent)))
        {
            toggleComponent.Activated = true;
            TryComp<AppearanceComponent>(ent, out var appComp);
            _appearanceSystem.SetData(ent, MetalDetectorVisuals.MetalDetectorActivated, true);
            _appearanceSystem.SetData(ent, ToggleableVisuals.Enabled, true, appComp);
            var toggledEvent = new ItemToggledEvent(false, true, ent);
            ent.Comp.EndOfSirenSound = _timing.CurTime + ent.Comp.SirenRunTime;
            RaiseLocalEvent(ent, ref toggledEvent);

            _deviceLink.InvokePort(ent, ent.Comp.TriggerPort);
        }
    }

    private void HandleStepOffTriggered(Entity<MetalDetectorComponent> ent, ref StepTriggeredOffEvent args)
    {
        // Start timer to deactivate the siren
        ent.Comp.IsSirenRunning = TryComp<ItemToggleComponent>(ent, out var toggleComponent) && toggleComponent.Activated;
    }

    private void HandleStepTriggerAttempt(Entity<MetalDetectorComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = TryComp<ApcPowerReceiverComponent>(ent, out var receiver) && receiver.Powered;
    }

    private void OnEmagged(Entity<MetalDetectorComponent> metalDetectorComponent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(metalDetectorComponent.Owner, EmagType.Interaction))
            return;

        args.Handled = true;
    }

    private bool CheckForContraband(EntityUid characterUid)
    {
        // In it's just a contraband item
        if (HasComp<ContrabandComponent>(characterUid))
            return true;

        _idSystem.TryFindIdCard(characterUid, out var idCard);

        if (_containerSystem.TryGetContainer(characterUid, ImplanterComponent.ImplantSlotId, out var implants))
        {
            var storageImplanter = implants.ContainedEntities.ToList().Find(HasComp<StorageComponent>);

            if (TryComp<StorageComponent>(storageImplanter, out var storage))
            {
                foreach (var stored in storage.Container.ContainedEntities)
                {
                    IsEntityContraband(stored, idCard);
                }
            }
        }

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(characterUid))
        {
            IsEntityContraband(item, idCard);
        }

        return false;
    }

    private bool IsEntityContraband(EntityUid item, Entity<IdCardComponent> icCard)
    {
        if (!TryComp<ContrabandComponent>(item, out var contrabandComp))
            return false;

        return icCard == null || !icCard.Comp.JobDepartments.Intersect(contrabandComp.AllowedDepartments).Any();
    }

}
