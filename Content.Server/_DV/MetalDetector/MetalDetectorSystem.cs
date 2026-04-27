using System.Linq;
using Content.Server.Power.Components;
using Content.Shared._DV.MetalDetector;
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

namespace Content.Server._DV.MetalDetector;

/// <summary>
/// WIP
/// </summary>
public sealed class MetalDetectorSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

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
            if (!comp.StartRunTime || !TryComp<ItemToggleComponent>(ent, out var toggle) || !toggle.Activated)
                continue;

            var powered = TryComp<ApcPowerReceiverComponent>(ent, out var receiver) && receiver.Powered;

            comp.CurrentRunTime += frameTime;
            TryComp<AppearanceComponent>(ent, out var appComp);

            if (comp.CurrentRunTime >= comp.RunTime || !powered)
            {
                comp.CurrentRunTime = 0.0f;
                comp.StartRunTime = false;
                var toggledEvent = new ItemToggledEvent(false, false, ent);
                toggle.Activated = false;
                _appearanceSystem.SetData(ent, MetalDetectorVisuals.MetalDetectorActivated, false);
                _appearanceSystem.SetData(ent, ToggleableVisuals.Enabled, false, appComp);
                RaiseLocalEvent(ent, ref toggledEvent);
            }
        }
    }

    private void HandleStepOnTriggered(EntityUid uid, MetalDetectorComponent component, ref StepTriggeredOnEvent args)
    {
        if (component.StartRunTime)
        {
            component.CurrentRunTime = 0.0f;
            return;
        }

        var random = new Random();
        if (TryComp<ItemToggleComponent>(uid, out var toggleComponent) && (CheckForContraband(args.Tripper)
                || random.NextFloat(0.0f, 100.0f) < component.FalsePositiveChance
                || HasComp<EmaggedComponent>(uid)))
        {
            toggleComponent.Activated = true;
            TryComp<AppearanceComponent>(uid, out var appComp);
            _appearanceSystem.SetData(uid, MetalDetectorVisuals.MetalDetectorActivated, true);
            _appearanceSystem.SetData(uid, ToggleableVisuals.Enabled, true, appComp);
            var toggledEvent = new ItemToggledEvent(false, true, uid);
            RaiseLocalEvent(uid, ref toggledEvent);
        }
    }

    private void HandleStepOffTriggered(EntityUid uid, MetalDetectorComponent component, ref StepTriggeredOffEvent args)
    {
        // Start timer to deactivate the siren
        component.StartRunTime = TryComp<ItemToggleComponent>(uid, out var toggleComponent) && toggleComponent.Activated;
    }

    private void HandleStepTriggerAttempt(EntityUid uid,
        MetalDetectorComponent component,
        ref StepTriggerAttemptEvent args)
    {
        args.Continue = TryComp<ApcPowerReceiverComponent>(uid, out var receiver) && receiver.Powered;
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
        var foundIdCArd = _idSystem.TryFindIdCard(characterUid, out var idCard);

        if (_containerSystem.TryGetContainer(characterUid, ImplanterComponent.ImplantSlotId, out var implants))
        {
            var storageImplanter = implants.ContainedEntities.ToList().Find(HasComp<StorageComponent>);

            if (TryComp<StorageComponent>(storageImplanter, out var storage))
            {
                foreach (var stored in storage.Container.ContainedEntities)
                {
                    if (!TryComp<ContrabandComponent>(stored, out var contrabandComp))
                        continue;

                    if (!foundIdCArd)
                        return true;

                    return !idCard.Comp.JobDepartments.Intersect(contrabandComp.AllowedDepartments).Any();
                }
            }
        }

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(characterUid))
        {
            if (!TryComp<ContrabandComponent>(item, out var contrabandComp))
                continue;

            if (!foundIdCArd)
                return true;

            return !idCard.Comp.JobDepartments.Intersect(contrabandComp.AllowedDepartments).Any();
        }

        return false;
    }
}
