using System.Linq;
using System.Numerics;
using Content.Server._DV.Planet;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Shared._DV.Planet;
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
    [Dependency] private readonly PlanetSystem _planet = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;

    public static readonly ProtoId<PlanetPrototype> FoldPlanet = "AkashicFold";
    private readonly ResPath _baseGridPath = new("Maps/_DV/AkashicFold/akashic_base.yml");
    private static EntityUid? _map;
    private static MapId _mapId;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        //SubscribeLocalEvent<AkashicFoldSpawnerComponent, MapInitEvent>(OnMapInit);
        //SubscribeLocalEvent<AkashicFoldSpawnerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        _map = _planet.LoadPlanet(FoldPlanet, _baseGridPath);
        if (!TryComp<MapComponent>(_map, out var mapComp))
            return;

        _mapId = mapComp.MapId;
        PlaceRuins();
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
            var dist = 10; // ?????

            var offset = _random.NextVector2(dist, dist * 2.5f);
            var randomer = _random.NextVector2(dist, dist * 5f);
            var ruin = _random.PickAndTake(ruins);
            if (!_mapLoader.TryLoadGrid(_mapId,
                    ruin.MapPath,
                    out var grid,
                    offset: Vector2.Zero + offset + randomer,
                    rot: Angle.Zero))
                return;

            // i am lowkey crashing out
            // todo: move this shit to PlanetSystem probably
            List<(Vector2i, Tile)> setTiles = new();
            var aabb = Comp<MapGridComponent>(grid.Value).LocalAABB;
            _biome.ReserveTiles(_map.Value, aabb.Enlarged(0.2f), setTiles);
        }
    }

    // okay, try number 2. this is all just me throwing shit at a wall until it sticks, will clean/reorganize once it works
    // stealing everything from goob Content.Server/_Lavaland/Procedural/Systems/LavalandSystem.Ruins.cs
    // what is a LavalandPreloader. more importantly, WHY is a LavalandPreloader
    //private bool LoadGridRuin()
}
