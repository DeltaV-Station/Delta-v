using Content.Server.DeltaV.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;

namespace Content.Server.DeltaV.Objectives.Systems;

/// <summary>
///     Handles the kill fellow traitor objective.
/// </summary>
public sealed class KillFellowTraitorObjectiveSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PickRandomTraitorComponent, ObjectiveAssignedEvent>(OnTraitorKillAssigned);
    }

    private void OnTraitorKillAssigned(EntityUid uid, PickRandomTraitorComponent comp, ref ObjectiveAssignedEvent args)
    {
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
        {
            Log.Error($"Missing components for {uid}.");
            args.Cancelled = true;
            return;
        }

        // Target already assigned
        if (target.Target != null)
        {
            Log.Error($"Target already assigned for {uid}.");
            args.Cancelled = true;
            return;
        }

        var traitors = _traitorRule.GetOtherTraitorMindsAliveAndConnected(args.Mind);

        List<EntityUid> validTraitorMinds = [];

        // Going through each OTHER traitor
        foreach (var traitor in traitors)
        {
            // Assume it will be a valid traitor.
            validTraitorMinds.Add(traitor.Id);

            // Going through each of OUR objectives.
            foreach (var objective in args.Mind.Objectives)
            {
                // If one of OUR objectives already targets a traitor, remove them from the vaild list.
                if (TryComp<TargetObjectiveComponent>(objective, out var targetComp) && targetComp.Target == traitor.Id)
                    validTraitorMinds.RemoveAt(validTraitorMinds.Count - 1);
            }
        }

        // No other traitors
        if (validTraitorMinds.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(uid, _random.Pick(validTraitorMinds), target);
    }
}
