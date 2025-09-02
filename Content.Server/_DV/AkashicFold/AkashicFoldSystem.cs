using System.Linq;
using System.Numerics;
using Content.Server._DV.Planet;
using Content.Server.Decals;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Shared._DV.Planet;
using Content.Shared.Decals;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._DV.AkashicFold;

// entity component system? more like entity nothing system amirite
// stealing coscult code basically
// todo: delete these comments so i don't seem like a complete nerd
public sealed class AkashicFoldSystem : EntitySystem
{
    // todo: alphabetically sort... or whatever sort you're supposed to use here
    [Dependency] private readonly PlanetSystem _planet = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSys = default!; // TODO: rename this when next todo is done
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DecalSystem _decals = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;

    private EntityQuery<MapGridComponent> _gridQuery;

    public static readonly ProtoId<PlanetPrototype> FoldPlanet = "AkashicFold";
    private readonly ResPath _baseGridPath = new("Maps/_DV/AkashicFold/akashic_base.yml");
    private static EntityUid? _map; // TODO: get rid of these evil fucking static variables
    private static MapId _mapId;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        _gridQuery = GetEntityQuery<MapGridComponent>();

        //SubscribeLocalEvent<AkashicFoldSpawnerComponent, MapInitEvent>(OnMapInit);
        //SubscribeLocalEvent<AkashicFoldSpawnerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        /*_map = _planet.LoadPlanet(FoldPlanet, _baseGridPath);
        if (!TryComp<MapComponent>(_map, out var mapComp))
            return;

        _mapId = mapComp.MapId;*/
        LoadFold();
        //PlaceRuins();
    }

    // like _planet.LoadPlanet but awesome
    private void LoadFold()
    {
        var map = _planet.SpawnPlanet(FoldPlanet, runMapInit: false);
        _map = map;
        _mapId = Comp<MapComponent>(_map.Value).MapId;

        LoadFoldGrid(_baseGridPath, new Vector2i(0, 0)); // base camp, always centered
        PlaceRuins();

        _mapSys.InitializeMap(map);
    }

    private void PlaceRuins()
    {
        if (_map == null)
            return;

        // counting on being able to place dungeons post-initialization (pray for me)
        // stealing a bit (a lot) of this from DebrisSpawnerRule. i dont think touching that would be a good idea
        // inb4 upstream has something for exactly this and I don't know about it
        // get all AkashicRuin protos
        var ruins = _proto.EnumeratePrototypes<AkashicRuinPrototype>().ToList();

        // if i leave in this hardcoded count and i dont take it out before PRing this, deltanedas gets to legally murder me
        // todo: get world AABBs so things don't explode
        for (var i = 0; i < 5; i++)
        {
            var ruin = _random.PickAndTake(ruins);
            var coords = _random.NextVector2(200f);
            LoadGridRuin(ruin, new Vector2i((int)coords.X, (int)coords.Y)); // lol?
        }
    }

    // okay, try number 2. this is all just me throwing shit at a wall until it sticks, will clean/reorganize once it works
    // stealing everything from goob Content.Server/_Lavaland/Procedural/Systems/LavalandSystem.Ruins.cs
    // what is a LavalandPreloader. more importantly, WHY is a LavalandPreloader
    // since we should be tying structure coordinates to station coordinates, we can forego a preloader/boundary checks
    // and just assume that station mappers aren't actively trying to murder us (:clueless:)
    private bool LoadGridRuin(AkashicRuinPrototype ruin, Vector2i coords)
    {
        //you know what. not sure why i have a function for this
        LoadFoldGrid(ruin.MapPath, coords);

        return true; // yeah fuck yeah dude hell yeah
    }

    // making this its own thing since the basecamp gonna use it too
    private void LoadFoldGrid(ResPath path, Vector2i coords)
    {
        if (_map == null)
            return;

        if (!_mapLoader.TryLoadGrid(_mapId, path, out var spawnedBoundedGrid))
        {
            Log.Error($"Failed to load ruin {path.Filename}? what the fuck?");
            return;
        }

        // goob comment: "It's not useless!" ok if you say so dude
        var spawned = spawnedBoundedGrid.Value;

        _transform.SetCoordinates(spawned, new EntityCoordinates(_map.Value, coords));

        // oh god
        GoidaMerge(spawned, (_map.Value, _gridQuery.Comp(_map.Value)), coords, out _); // oh GOD

        _biome.ReserveTiles(_map.Value, Comp<MapGridComponent>(spawned).LocalAABB, new List<(Vector2i, Tile)>(), Comp<BiomeComponent>(_map.Value), Comp<MapGridComponent>(_map.Value)); // erm
    }

    // stolen from goob, god please help
    // thank you goob wizards for being awesome
    private readonly List<(Vector2i, Tile)> _tiles = new();

        private void GoidaMerge(
        Entity<MapGridComponent> grid,
        Entity<MapGridComponent> mapgrid,
        Vector2i offset,
        out Box2 usedSpace,
        HashSet<Vector2i>? reservedTiles = null)
    {
        usedSpace = grid.Comp.LocalAABB.Translated(offset);
        var center = usedSpace.Center;
        var roomTransform = Matrix3Helpers.CreateTranslation(center.X, center.Y);

        // Copy all tiles
        _tiles.Clear();

        var tiles = _mapSys.GetAllTiles(grid.Owner, grid.Comp).ToList();
        foreach (var tileRef in tiles)
        {
            _tiles.Add((tileRef.GridIndices + offset, tileRef.Tile));
        }

        _mapSys.SetTiles(mapgrid.Owner, mapgrid.Comp, _tiles);

        // Teleport all entities
        var ents = new HashSet<Entity<TransformComponent>>();
        _lookup.GetChildEntities(grid, ents);
        foreach (var (teleportEnt, xform) in ents)
        {
            var anchored = xform.Anchored;
            var newPos = new EntityCoordinates(mapgrid.Owner, xform.LocalPosition + offset);
            _transform.SetParent(teleportEnt, mapgrid);
            _transform.SetCoordinates(teleportEnt, newPos);

            if (anchored)
                _transform.AnchorEntity(teleportEnt);
        }

        // Spawn decals
        if (TryComp<DecalGridComponent>(grid.Owner, out var loadedDecals))
        {
            EnsureComp<DecalGridComponent>(mapgrid);
            foreach (var (_, decal) in _decals.GetDecalsIntersecting(grid.Owner, usedSpace, loadedDecals))
            {
                // Offset by 0.5 because decals are offset from bot-left corner
                // So we convert it to center of tile then convert it back again after transform.
                // Do these shenanigans because 32x32 decals assume as they are centered on bottom-left of tiles.
                var position = Vector2.Transform(decal.Coordinates + grid.Comp.TileSizeHalfVector - center,
                    roomTransform);
                position -= grid.Comp.TileSizeHalfVector;

                if (reservedTiles?.Contains(position.Floored()) == true)
                    continue;

                // Umm uhh I love decals so uhhhh idk what to do about this
                var angle = decal.Angle.Reduced();

                // Adjust because 32x32 so we can't rotate cleanly
                // Yeah idk about the uhh vectors here but it looked visually okay but they may still be off by 1.
                // Also EyeManager.PixelsPerMeter should really be in shared.
                if (angle.Equals(Math.PI))
                {
                    position += new Vector2(-1f / 32f, 1f / 32f);
                }
                else if (angle.Equals(-Math.PI / 2f))
                {
                    position += new Vector2(-1f / 32f, 0f);
                }
                else if (angle.Equals(Math.PI / 2f))
                {
                    position += new Vector2(0f, 1f / 32f);
                }
                else if (angle.Equals(Math.PI * 1.5f))
                {
                    // I hate this but decals are bottom-left rather than center position and doing the
                    // matrix ops is a PITA hence this workaround for now; I also don't want to add a stupid
                    // field for 1 specific op on decals
                    if (decal.Id != "DiagonalCheckerAOverlay" &&
                        decal.Id != "DiagonalCheckerBOverlay")
                    {
                        position += new Vector2(-1f / 32f, 0f);
                    }
                }

                var tilePos = position.Floored();

                // Fallback because uhhhhhhhh yeah, a corner tile might look valid on the original
                // but place 1 nanometre off grid and fail the add.
                if (!_mapSys.TryGetTileRef(grid, grid, tilePos, out var tileRef) || tileRef.Tile.IsEmpty)
                {
                    _mapSys.SetTile(grid,
                        grid,
                        tilePos,
                        _tile.GetVariantTile((ContentTileDefinition) _tiledef[DungeonSystem.FallbackTileId],
                            _random.GetRandom()));
                }

                _decals.TryAddDecal(
                    decal.Id,
                    new EntityCoordinates(grid, position),
                    out _,
                    decal.Color,
                    angle,
                    decal.ZIndex,
                    decal.Cleanable);
            }
        }
    }
}
