using Content.Server._DV.Planet;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Shared._DV.AkashicFold;
using Content.Shared._DV.Planet;
using Content.Shared.Light.Components;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DV.AkashicFold;

public sealed class AkashicFoldSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly PlanetSystem _planet = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly ProtoId<PlanetPrototype> _foldPlanet = "AkashicFold";
    private readonly ResPath _baseGridPath = new("Maps/_DV/AkashicFold/akashic_base.yml");
    private static EntityUid? _mapEntUid;
    private static MapId _mapId;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        LoadFold();
    }

    private void LoadFold()
    {
        var map = _planet.SpawnPlanet(_foldPlanet, runMapInit: false);
        _mapEntUid = map;
        _mapId = Comp<MapComponent>(_mapEntUid.Value).MapId;

        LoadFoldGrid(_baseGridPath, new Vector2i(0, 0)); // base camp, always centered
        PlaceRuins();

        _map.InitializeMap(map);
    }

    // placeholder until smarter ruin placement is done
    private void PlaceRuins()
    {
        if (_mapEntUid == null)
            return;

        // to get available ruins: _proto.EnumeratePrototypes<AkashicRuinPrototype>().ToList();
    }

    private bool LoadGridRuin(AkashicRuinPrototype ruin, Vector2i coords)
    {
        if(!(LoadFoldGrid(ruin.MapPath, coords) is { } ruinGrid))
            return false;

        if(!ruin.RoofEnabled)
            RemComp<ImplicitRoofComponent>(ruinGrid);

        return true;
    }

    private Entity<MapGridComponent>? LoadFoldGrid(ResPath path, Vector2i coords)
    {
        if (_mapEntUid == null)
            return null;

        if (!_mapLoader.TryLoadGrid(_mapId, path, out var spawnedBoundedGrid))
        {
            Log.Error($"Failed to load Fold grid {path.Filename}!");
            return null;
        }

        var spawned = spawnedBoundedGrid.Value;
        _transform.SetCoordinates(spawned, new EntityCoordinates(_mapEntUid.Value, coords));
        _biome.ReserveTiles(_mapEntUid.Value,
            Comp<MapGridComponent>(spawned).LocalAABB,
            new List<(Vector2i, Tile)>(),
            Comp<BiomeComponent>(_mapEntUid.Value),
            Comp<MapGridComponent>(_mapEntUid.Value));

        return spawned;
    }
}
