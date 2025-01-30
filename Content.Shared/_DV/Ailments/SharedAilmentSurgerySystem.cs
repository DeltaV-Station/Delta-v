using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Ailments;

public abstract partial class SharedAilmentSurgerySystem : EntitySystem
{
    [Dependency] private readonly SharedAilmentSystem _ailments = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryHasAilmentConditionComponent, SurgeryValidEvent>(OnAilmentConditionValid);
        SubscribeLocalEvent<SurgeryStepAilmentTransitionComponent, SurgeryStepCompleteCheckEvent>(OnCheckTransitionComplete);
        SubscribeLocalEvent<SurgeryStepAilmentTransitionComponent, SurgeryStepEvent>(OnAilmentTransition);
    }

    private void OnAilmentConditionValid(Entity<SurgeryHasAilmentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp<AilmentComponent>(args.Part, out var comp))
        {
            args.Cancelled = true;
            return;
        }

        var hasAilment = comp.ActiveAilments.GetValueOrDefault(ent.Comp.Pack) == ent.Comp.Ailment;
        args.Cancelled = !hasAilment;
    }

    private void OnCheckTransitionComplete(Entity<SurgeryStepAilmentTransitionComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<AilmentComponent>(args.Part, out var ailments))
            return;

        var eeba = new EntityEffectBaseArgs(args.Body, EntityManager);
        var canTakeTransition = _ailments.ValidateTransition(new Entity<AilmentComponent>(args.Part, ailments), ent.Comp.Pack, ent.Comp.Transition, eeba);
        args.Cancelled = canTakeTransition;
    }

    private void OnAilmentTransition(Entity<SurgeryStepAilmentTransitionComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<AilmentComponent>(args.Part, out var ailments))
            return;

        var eeba = new EntityEffectBaseArgs(args.Body, EntityManager);
        _ailments.TryTransition(new Entity<AilmentComponent>(args.Part, ailments), ent.Comp.Pack, ent.Comp.Transition, eeba);
    }
}
