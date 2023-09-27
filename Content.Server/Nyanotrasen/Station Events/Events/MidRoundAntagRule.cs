using System.Linq;
using Robust.Shared.Random;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;

namespace Content.Server.StationEvents.Events;

internal sealed class MidRoundAntagRule : StationEventSystem<MidRoundAntagRuleComponent>
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    protected override void Started(EntityUid uid, MidRoundAntagRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var spawnLocations = EntityManager.EntityQuery<MidRoundAntagSpawnLocationComponent, TransformComponent>().ToList();
        var backupSpawnLocations = EntityManager.EntityQuery<VentCritterSpawnLocationComponent, TransformComponent>().ToList();

        TransformComponent? spawn = new();

        if (spawnLocations.Count > 0)
        {
            var spawnLoc = _robustRandom.Pick(spawnLocations);
            spawn = spawnLoc.Item2;
        } else if (backupSpawnLocations.Count > 0)
        {
            var spawnLoc = _robustRandom.Pick(backupSpawnLocations);
            spawn = spawnLoc.Item2;
        }

        if (spawn == null)
            return;

        if (spawn.GridUid == null)
        {
            return;
        }

        Spawn(_robustRandom.Pick(component.MidRoundAntags), spawn.Coordinates);
    }
}
