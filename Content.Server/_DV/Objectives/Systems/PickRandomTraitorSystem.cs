using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives.Systems;
using Content.Shared._DV.Reputation;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles picking a random traitor for <see cref="PickRandomTraitorComponent"/>.
/// </summary>
public sealed class PickRandomTraitorSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReputationSystem _reputation = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;

    private List<EntityUid> _validMinds = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PickRandomTraitorComponent, ObjectiveAssignedEvent>(OnAssigned);
    }

    private void OnAssigned(Entity<PickRandomTraitorComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var target = Comp<TargetObjectiveComponent>(ent);

        // Target already assigned
        if (target.Target != null)
        {
            Log.Error($"Target already assigned for {ToPrettyString(ent)}");
            args.Cancelled = true;
            return;
        }

        var traitors = _traitorRule.GetOtherTraitorMindsAliveAndConnected(args.Mind);

        _validMinds.Clear();

        // Going through each OTHER traitor
        foreach (var traitor in traitors)
        {
            // check reputation first
            var reputation = _reputation.GetMindReputation(traitor.Id) ?? 0;
            if (reputation < ent.Comp.MinReputation)
                continue;

            // then see if they have enough contracts
            if (ent.Comp.MinContracts > 0)
            {
                var pda = CompOrNull<MindReputationComponent>(traitor.Id)?.Pda;
                var contracts = CompOrNull<ContractsComponent>(pda)?.Objectives
                    .Count(o => o != null) ?? 0;
                if (contracts < ent.Comp.MinContracts)
                    continue;
            }

            _validMinds.Add(traitor.Id);
        }

        // No other traitors
        if (_validMinds.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(ent, _random.Pick(_validMinds), target);
    }
}
