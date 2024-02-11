using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Station.Components;
using Content.Shared.Salvage;
using Content.Shared.Random.Helpers;
using System.Linq;

namespace Content.Server.StationEvents.Events;

public sealed class PirateRadioSpawnRule : StationEventSystem<PirateRadioSpawnRuleComponent>
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    protected override void Started(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        //Start of Syndicate Listening Outpost spawning system
        base.Started(uid, component, gameRule, args);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var aabbs = EntityQuery<StationDataComponent>().SelectMany(x =>
                x.Grids.Select(x =>
                    xformQuery.GetComponent(x).WorldMatrix.TransformBox(_mapManager.GetGridComp(x).LocalAABB)))
            .ToArray();
        if (aabbs.Length < 1) return;
        var aabb = aabbs[0];

        for (var i = 1; i < aabbs.Length; i++)
        {
            aabb.Union(aabbs[i]);
        }
        var distanceModifier = Math.Clamp(component.DistanceModifier, 1, 25);
        var a = MathF.Max(aabb.Height / 2f, aabb.Width / 2f) * distanceModifier;
        var randomoffset = _random.NextVector2(a, a * 2.5f);
        var outpostOptions = new MapLoadOptions
        {
            Offset = aabb.Center + randomoffset,
            LoadMap = false,
        };
        if (!_map.TryLoad(GameTicker.DefaultMap, component.PirateRadioShuttlePath, out var outpostids, outpostOptions)) return;
        //End of Syndicate Listening Outpost spawning system

        //Start of Debris Field Generation
        var debrisCount = Math.Clamp(component.DebrisCount, 0, 6);
        if (debrisCount == 0) return;
        var debrisDistanceModifier = Math.Clamp(component.DebrisDistanceModifier, 3, 10);
        foreach (var id in outpostids)
        {
            if (!TryComp<MapGridComponent>(id, out var grid)) return;
            var outpostaabb = _entities.GetComponent<TransformComponent>(id).WorldMatrix.TransformBox(grid.LocalAABB);
            var b = MathF.Max(outpostaabb.Height / 2f, aabb.Width / 2f) * debrisDistanceModifier;
            var k = 1;
            while (k < debrisCount + 1)
            {
                var debrisRandomOffset = _random.NextVector2(b, b * 2.5f);
                var randomer = _random.NextVector2(b, b * 5f); //Second random vector to ensure the outpost isn't perfectly centered in the debris field
                var debrisOptions = new MapLoadOptions
                {
                    Offset = outpostaabb.Center + debrisRandomOffset + randomer,
                    LoadMap = false,
                };

                var salvageProto = _random.Pick(_prototypeManager.EnumeratePrototypes<SalvageMapPrototype>().ToList());
                _map.TryLoad(GameTicker.DefaultMap, salvageProto.MapPath.ToString(), out _, debrisOptions);
                k++;
            }
        }
        //End of Debris Field generation
    }

    protected override void Ended(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (component.AdditionalRule != null)
            GameTicker.EndGameRule(component.AdditionalRule.Value);
    }
}
