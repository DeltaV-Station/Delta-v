using Content.Shared._DV.Trigger.Components;
using Content.Shared._Funkystation.Fluids;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Stunnable;
using Content.Shared.Trigger;

namespace Content.Shared._DV.Trigger.Systems.OnTriggerSystems;

public sealed class SpillOnTriggerSystem : XOnTriggerSystem<SpillOnTriggerComponent>
{
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;

    protected override void OnTrigger(Entity<SpillOnTriggerComponent> spiller, EntityUid target, ref TriggerEvent args)
    {
        if (!_solutionContainer.ResolveSolution(spiller.Owner, spiller.Comp.SolutionName, ref spiller.Comp.Solution, out var solution)
            || solution.Volume <= FixedPoint2.Zero)
            return;

        var targetSlots = HasComp<KnockedDownComponent>(target)
            ? spiller.Comp.ProneTargetSlots
            : spiller.Comp.TargetSlots;

        var spilledEvent = new SpilledOnEvent(spiller, solution, targetSlots);
        RaiseLocalEvent(target, spilledEvent);

        args.Handled = true;
    }
}
