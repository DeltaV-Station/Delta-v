using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Server.RoundEnd;
using Content.Server.Station.Components;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.CCVar;
using System.Linq;
using System.Numerics;

namespace Content.Server.StationEvents.Events;

public sealed class PirateRadioSpawnRule : StationEventSystem<PirateRadioSpawnRuleComponent>
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly SalvageSystem _salvage = default!;

    protected override void Started(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var xformQuery = GetEntityQuery<TransformComponent>();
        //Find where the station is and get a bounding box
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
        var a = MathF.Max(aabb.Height / 2f, aabb.Width / 2f) * component.DistanceModifier;
        var randomoffset = _random.NextVector2(a, a * 2.5f);
        var OutpostOptions = new MapLoadOptions
        {
            Offset = aabb.Center + randomoffset,
            LoadMap = false,
        };
        _map.TryLoad(GameTicker.DefaultMap, component.PirateRadioShuttlePath, out var Outpostids, OutpostOptions);

        //Now do it again but for the outpost
        if (Outpostids == null) return;
        foreach (var id in Outpostids)
        {
            if (!TryComp<MapGridComponent>(id, out var grid)) return;
            int k = 1;
            while (k < component.DebrisCount + 1)
            {
                var outpostaabb = _entities.GetComponent<TransformComponent>(id).WorldMatrix.TransformBox(grid.LocalAABB);
                var b = MathF.Max(outpostaabb.Height / 2f, aabb.Width / 2f) * component.DebrisDistanceModifier;
                var debrisRandomOffset = _random.NextVector2(b, b * 2.5f);
                var randomer = _random.NextVector2(b, b * 5f);
                var DebrisOptions = new MapLoadOptions
                {
                    Offset = outpostaabb.Center + debrisRandomOffset + randomer,
                    LoadMap = false,
                };
                var forcedSalvage = _configurationManager.GetCVar(CCVars.SalvageForced);
                var salvageProto = string.IsNullOrWhiteSpace(forcedSalvage)
                    ? _random.Pick(_prototypeManager.EnumeratePrototypes<SalvageMapPrototype>().ToList())
                    : _prototypeManager.Index<SalvageMapPrototype>(forcedSalvage);
                _map.TryLoad(GameTicker.DefaultMap, salvageProto.MapPath.ToString(), out _, DebrisOptions);
                k++;
            }
        }
    }

    protected override void Ended(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (component.AdditionalRule != null)
            GameTicker.EndGameRule(component.AdditionalRule.Value);
    }
}
