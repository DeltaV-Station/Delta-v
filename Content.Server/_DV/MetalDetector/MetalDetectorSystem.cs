using Content.Server.Power.Components;
using Content.Shared._DV.MetalDetector;
using Content.Shared.Contraband;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Power.Components;
using Content.Shared.StepTrigger.Systems;

namespace Content.Server._DV.MetalDetector;

/// <summary>
/// WIP
/// </summary>
public sealed class MetalDetectorSystem : EntitySystem
{
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

            if (comp.CurrentRunTime >= comp.RunTime)
            {
                comp.CurrentRunTime = 0.0f;
                comp.StartRunTime = false;
                var toggledEvent = new ItemToggledEvent(false, false, ent);
                RaiseLocalEvent(ent, ref toggledEvent);
                toggle.Activated = false;
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

        if (TryComp<ItemToggleComponent>(uid, out var toggleComponent) && HasContraband(args.Tripper))
        {
            var toggledEvent = new ItemToggledEvent(false, true, uid);
            RaiseLocalEvent(uid, ref toggledEvent);
            toggleComponent.Activated = true;
        }
    }

    private void HandleStepOffTriggered(EntityUid uid, MetalDetectorComponent component, ref StepTriggeredOffEvent args)
    {
        // Start timer to deactivate the siren
        component.StartRunTime = true;

    }

    private void HandleStepTriggerAttempt(EntityUid uid,
        MetalDetectorComponent component,
        ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private bool HasContraband(EntityUid uid)
    {
        return false;
    }
}
