using System.Linq;
using Content.Shared._DV.MetalDetector;
using Content.Shared.Access.Systems;
using Content.Shared.Contraband;
using Content.Shared.Implants.Components;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Power;
using Content.Shared.Random;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Storage;
using Robust.Server.GameObjects;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetalDetectorComponent, StepTriggeredOnEvent>(HandleStepOnTriggered);
        SubscribeLocalEvent<MetalDetectorComponent, StepTriggeredOffEvent>(HandleStepOffTriggered);
        SubscribeLocalEvent<MetalDetectorComponent, StepTriggerAttemptEvent>(HandleStepTriggerAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        using var query = EntityQueryEnumerator<MetalDetectorComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (!comp.StartRunTime || !TryComp<ItemToggleComponent>(ent, out var toggle) || !toggle.Activated)
                return;

            comp.CurrentRunTime += frameTime;

            if (comp.CurrentRunTime < comp.RunTime)
                return;

            comp.CurrentRunTime = 0.0f;
            comp.StartRunTime = false;
            var toggledEvent = new ItemToggledEvent(false, false, ent);
            toggle.Activated = false;
            _appearanceSystem.SetData(ent, MetalDetectorVisuals.MetalDetectorActivated, false);
            RaiseLocalEvent(ent, ref toggledEvent);
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
                || random.NextFloat(0.0f, 100.0f) < component.FalsePositiveChance))
        {
            toggleComponent.Activated = true;
            _appearanceSystem.SetData(uid, MetalDetectorVisuals.MetalDetectorActivated, true);
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
        args.Continue = true;
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
