using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.StationEvents.Events;

public sealed class MidRoundAntagRule : StationEventSystem<MidRoundAntagRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Started(EntityUid uid, MidRoundAntagRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var spawnLocations = EntityQuery<MidRoundAntagSpawnLocationComponent, TransformComponent>().ToList();
        var backupSpawnLocations = EntityQuery<VentCritterSpawnLocationComponent, TransformComponent>().ToList();

        TransformComponent? spawn = new();

        if (spawnLocations.Count > 0)
        {
            var spawnLoc = _random.Pick(spawnLocations);
            spawn = spawnLoc.Item2;
        } else if (backupSpawnLocations.Count > 0)
        {
            var spawnLoc = _random.Pick(backupSpawnLocations);
            spawn = spawnLoc.Item2;
        }

        if (spawn?.GridUid == null)
            return;

        var proto = _random.Pick(component.MidRoundAntags);
        Log.Info($"Spawning midround antag {proto} at {spawn.Coordinates}");
        Spawn(proto, spawn.Coordinates);
    }
}
