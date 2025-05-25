using Content.Server._DV.Objectives.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Nuke;
using Content.Server.Objectives.Systems;
using Content.Server.Station.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._DV.Objectives.Systems;

public sealed class NukeStationConditionSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeExplodedEvent>(OnNukeExploded);
    }

    private void OnNukeExploded(NukeExplodedEvent ev)
    {
        var nukeOpsQuery = EntityQueryEnumerator<NukeopsRuleComponent>();
        while (nukeOpsQuery.MoveNext(out _, out var nukeopsRule)) // this should only loop once.
        {
            if (!TryComp<StationDataComponent>(nukeopsRule.TargetStation, out var data))
                return;

            foreach (var grid in data.Grids)
            {
                if (grid != ev.OwningStation) // They nuked the target station!
                    continue;

                // Set all the objectives to true.
                var nukeStationQuery = EntityQueryEnumerator<NukeStationConditionComponent>();
                while (nukeStationQuery.MoveNext(out var uid, out _))
                {
                    _codeCondition.SetCompleted(uid);
                }
            }
        }
    }
}
