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
        //This handles spawning the Listening Outpost
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
        //Generates a potential spawning area, shaped like a square with rounded corners.
        //DistanceModifier allows for fine tuning of how large this area is.
        //The initial target distance(for DistanceModifier = 20f) is between 1km and 1.5km of the station.
        //But with the way NextVector2 works, it can be less than that
        //Do not for any reason set DistanceModifier greater than 25f or less than 1f. 
        var a = MathF.Max(aabb.Height / 2f, aabb.Width / 2f) * component.DistanceModifier;
        var randomoffset = _random.NextVector2(a, a * 2.5f);
        var outpostOptions = new MapLoadOptions
        {
            Offset = aabb.Center + randomoffset,
            LoadMap = false,
        };
        //Now spawn the Listening Outpost
        _map.TryLoad(GameTicker.DefaultMap, component.PirateRadioShuttlePath, out var outpostids, outpostOptions);

        //Now we generate the outpost's debris field
        if (outpostids == null) return;
        //Yes, this is a loop within a loop. Actually, foreach is just here to convert an array into a variable.
        //For whatever ungodly reason, Outpostids is an array of gridUids, even though it can only ever contain a single gridUid. 
        foreach (var id in outpostids)
        {
            if (!TryComp<MapGridComponent>(id, out var grid)) return;
            //Obtain the bounding box of the Listening Outpost
            var outpostaabb = _entities.GetComponent<TransformComponent>(id).WorldMatrix.TransformBox(grid.LocalAABB);
            var b = MathF.Max(outpostaabb.Height / 2f, aabb.Width / 2f) * component.DebrisDistanceModifier; //DebrisDistanceModifier controls how dense we want the debris field to be
            int k = 1;
            while (k < component.DebrisCount + 1) //DebrisCount defines how many wrecks to spawn
            {
                //Generate the region to spawn the debris
                //We have to remake this region for every wreck, otherwise they'll all spawn in the same place
                var debrisRandomOffset = _random.NextVector2(b, b * 2.5f);
                var randomer = _random.NextVector2(b, b * 5f); //Second random vector to ensure the outpost isn't perfectly centered in the debris field
                var debrisOptions = new MapLoadOptions
                {
                    Offset = outpostaabb.Center + debrisRandomOffset + randomer,
                    LoadMap = false,
                };


                var salvageProto = _random.Pick(_prototypeManager.EnumeratePrototypes<SalvageMapPrototype>().ToList());
                //And finally spawn a salvage wreck
                _map.TryLoad(GameTicker.DefaultMap, salvageProto.MapPath.ToString(), out _, debrisOptions);
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
