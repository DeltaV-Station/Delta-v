using Content.Server.Atmos.EntitySystems;
using Content.Server.Parallax;
using Content.Shared._DV.Planet;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Random;
using Robust.Shared.Maths;
using System;

namespace Content.Server._DV.Planet;

public sealed class PlanetSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    private readonly List<(Vector2i, Tile)> _setTiles = new();

    /// <summary>
    /// Spawn a planet map from a planet prototype.
    /// </summary>
    public EntityUid SpawnPlanet(ProtoId<PlanetPrototype> id, bool runMapInit = true)
    {
        var planet = _proto.Index(id);

        var map = _map.CreateMap(out _, runMapInit: runMapInit);
        _biome.EnsurePlanet(map, _proto.Index(planet.Biome), mapLight: planet.MapLight);

        // add each marker layer
        var biome = Comp<BiomeComponent>(map);
        foreach (var layer in planet.BiomeMarkerLayers)
        {
            _biome.AddMarkerLayer(map, biome, layer);
        }

        if (planet.AddedComponents is {} added)
            EntityManager.AddComponents(map, added);

        _atmos.SetMapAtmosphere(map, false, planet.Atmosphere);

        _meta.SetEntityName(map, Loc.GetString(planet.MapName));

        return map;
    }

    /// <summary>
    /// Spawns an initialized planet map from a planet prototype and loads a grid onto it.
    /// Returns the map entity if loading succeeded.
    /// </summary>
    public EntityUid? LoadPlanet(ProtoId<PlanetPrototype> id, ResPath path)
    {
        var map = SpawnPlanet(id, runMapInit: false);
        var mapId = Comp<MapComponent>(map).MapId;
        if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
        {
            Log.Error($"Failed to load planet grid {path} for planet {id}!");
            Del(map);
            return null;
        }

        // don't want rocks spawning inside the base
        _setTiles.Clear();
        var aabb = Comp<MapGridComponent>(grid.Value).LocalAABB;
        _biome.ReserveTiles(map, aabb.Enlarged(0.2f), _setTiles);

        _map.InitializeMap(map);
        return map;

    // Ruin generation for planet maps.
    private void TrySpawningRuins(EntityUid mapUid, MapId mapId, PlanetPrototype planet)
    {
        if (planet.RuinMaxCount <= 0)
            return;

        if (planet.RuinPaths.Count == 0 && planet.RareRuinPaths.Count == 0)
            return;

        List<ResPath> Ruins = new List<ResPath>(planet.RuinPaths);
        List<ResPath> rareRuins = new List<ResPath>(planet.RareRuinPaths);

        int totalRuinAvailable = Ruins.Count + rareRuins.Count;
        if (totalRuinAvailable == 0)
            return;

        _random.Shuffle(Ruins);
        _random.Shuffle(rareRuins);

        int minCount = Math.Clamp(planet.RuinMinCount, 0, totalRuinAvailable);
        int maxCount = Math.Clamp(planet.RuinMaxCount, minCount, totalRuinAvailable);

        int RuinSpawningCount = _random.Next(minCount, maxCount + 1);

        if (RuinSpawningCount == 0)
            return;

        List<ResPath> selectedRuins = new List<ResPath>();

        // Guaranteed rare ruins come first in selection!
        int guaranteedRare = Math.Clamp(planet.GuaranteedRareRuinCount, 0, Math.Min(selectedRuins, rareRuins.Count));

        for (int i = 0; i < guaranteedRare; i++)
        {
            if (rareRuins.Count == 0)
                break;

            ResPath ruin = rareRuins[rareRuins.Count - 1];
            rareRuins.RemoveAt(rareRuins.Count -1);
            selectedRuins.Add(ruin);
        }
        // Chance to select ruin from rare pool when filling up the slots.
        int rareChance = Math.Clamp(planet.RareRuinChance, 0, 100);

        while (selectedRuins.Count < RuinSpawningCount && (Ruins.Count > 0 || rareRuins.Count > 0))
        {
            bool pickRare = false;

            if (rareRuin.Count > 0)
            {
                if (Ruins.Count == 0)
                    pickRare = true;
                else if (_random.Next(100) < rareChance)
                    pickRare = true;
            }

            List<ResPath> pool = pickRare ? rareRuins : Ruins;

            if (pool.Count > 0)
                continue;

            ResPath ruin = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            selectedRuins.Add(ruin);
        }

        // Ruin placement prediction, keeps track of here ruins will be place to ensure no overlap or ruins that generate too closely.
        List<(Vector2 Center, float Radius)> placedRuins = new List<(Vector2 Center, float Radius)>();

        // TO-DO: Move these to PlanetPrototype fields
        float minDistance = 80f;
        float maxDistance = 450f;
        float minSeparation = 50f;
        int maxPlacementAttempts = 10;

        for (int i = 0; i < selectedRuins.Count; i++)
        {
            ResPath ruinPath = selectedRuins[i];

            // load the ruin onto a temp map first to calculate its bounding box for sepeartion.
            EntityUid probeMap = _map.CreateMap(out MapId probeMapId, runMapInit: false);
            float ruinRadius = 0f;

            bool probeSuccess = _mapLoader.TryLoadGrid(probeMapId, ruinPath, out EntityUid? probeGrid, offset: Vector2.Zero);

            if (probeSuccess)
            {
                Box2 probeAabb = Comp<MapGridComponent>(probeGrid!.Value).LocalAABB;
                ruinRadius = probeAabb.Size.Length() * 0.5f;

            Del(probeMap);

            if (!probeSuccess)
                continue;

            bool placed = false;

            // Try several random positions and pick the first one that doesn't overlap anything.
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                Vector2 randomOffset = _random.NextVector2(minDistance, maxDistance);
                Vector2 candidateCenter = center + randomOffset;
                bool separated = true;

                for (int j = 0; j < placedRuins.Count; j++)
                {
                    Vector2 otherCenter = placedRuins[j].Center;
                    float otherRadius = placedRuins[j].Radius;

                    float minAllowed = otherRadius + ruinRadius + minSeparation;

                    float dx = candidateCenter.X - otherCenter.X;
                    float dy = candidateCenter.Y - otherCenter.Y;
                    float distSquared = dx * dx + dy * dy;

                    if (distSquared < minAllowed * minAllowed)
                        separated = false;
                        break;
                }

                if (!separated)
                    continue;

                bool loadSuccess = _mapLoader.TryLoadGrid(mapId, ruinPath, out _, offset: candidateCenter);

                if (!loadSuccess)
                    continue;

                placedRuins.Add((candidateCenter, ruinRadius));
                placed = true;
                break;
            }

            // TO-DO: add error logging for ruins that could not be placed
            _ = placed;
        }
    }
}

