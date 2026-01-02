using System.Numerics;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Nyanotrasen.Research.SophicScribe;
using Content.Server.Research.Oracle;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.Events;

public sealed class GlimmerFoxfireSpawnRule : StationEventSystem<GlimmerFoxfireSpawnRuleComponent>
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    protected override void Started(EntityUid uid,
        GlimmerFoxfireSpawnRuleComponent comp,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var locations = new List<EntityCoordinates>();
        var queryScribe = EntityQueryEnumerator<SophicScribeComponent, TransformComponent>();
        var queryOracle = EntityQueryEnumerator<OracleComponent, TransformComponent>();

        while (queryScribe.MoveNext(out _, out var transform))
        {
            locations.Add(transform.Coordinates);
        }

        while (queryOracle.MoveNext(out _, out var transform))
        {
            locations.Add(transform.Coordinates);
        }

        if (locations.Count == 0)
            return;

        var selectedLocation = RobustRandom.Pick(locations);

        var amountToSpawn = RobustRandom.Next(comp.MinimumSpawned, comp.MaximumSpawned);
        for (var i = 0; i < amountToSpawn; i++)
        {
            var spawnLocation = selectedLocation.Offset(new Vector2(
                RobustRandom.Next(-comp.SpawnRange, comp.SpawnRange),
                RobustRandom.Next(-comp.SpawnRange, comp.SpawnRange)
            ));


            var color = Color.GhostWhite;
            if (comp.RandomColorList != null && comp.RandomColorList.Count != 0)
                color = RobustRandom.Pick(comp.RandomColorList);

            var fireEnt = Spawn(comp.FoxfirePrototype, spawnLocation);
            _light.SetColor(fireEnt, color);
        }
    }
}
