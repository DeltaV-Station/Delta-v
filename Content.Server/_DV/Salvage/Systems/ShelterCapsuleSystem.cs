using Content.Server.Fluids.EntitySystems;
using Content.Server.Procedural;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.Components;
using Content.Shared.Database;
using Content.Shared._DV.Salvage.Components;
using Content.Shared._DV.Salvage.Systems;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Server._DV.Salvage.Systems;

public sealed class ShelterCapsuleSystem : SharedShelterCapsuleSystem
{
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;

    public static readonly EntProtoId SmokePrototype = "Smoke";

    private EntityQuery<FixturesComponent> _fixturesQuery;
    private HashSet<EntityUid> _entities = new();

    public override void Initialize()
    {
        base.Initialize();

        _fixturesQuery = GetEntityQuery<FixturesComponent>();
    }

    protected override LocId? TrySpawnRoom(Entity<ShelterCapsuleComponent> ent)
    {
        var xform = Transform(ent);
        if (xform.GridUid is not {} gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return "shelter-capsule-error-space";

        var center = _map.LocalToTile(gridUid, grid, xform.Coordinates);
        var room = _proto.Index(ent.Comp.Room);
        var origin = center - room.Size / 2;

        // check that every tile it needs isn't blocked
        var mask = (int) CollisionGroup.MobMask;
        if (IsAreaBlocked(gridUid, center, room.Size, mask))
            return "shelter-capsule-error-obstructed";

        // check that it isn't on space or SpawnRoom will crash
        for (int y = 0; y < room.Size.Y; y++)
        {
            for (int x = 0; x < room.Size.X; x++)
            {
                var pos = origin + new Vector2i(x, y);
                var tile = _map.GetTileRef((gridUid, grid), pos);
                if (tile.Tile.IsEmpty)
                    return "shelter-capsule-error-space";
            }
        }

        var user = ent.Comp.User;
        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(user):user} expanded {ToPrettyString(ent):capsule} at {center} on {ToPrettyString(gridUid):grid}");

        _dungeon.SpawnRoom(gridUid,
            grid,
            origin,
            room,
            new Random(),
            null,
            clearExisting: true); // already checked for mobs and structures here

        var smoke = Spawn(SmokePrototype, xform.Coordinates);
        var spreadAmount = (int) room.Size.Length * 2;
        _smoke.StartSmoke(smoke, new Solution(), 3f, spreadAmount);

        QueueDel(ent);
        return null;
    }

    private bool IsAreaBlocked(EntityUid grid, Vector2i center, Vector2i size, int mask)
    {
        // This is scaled to 95 % so it doesn't encompass walls on other tiles.
        var aabb = Box2.CenteredAround(center, size * 0.95f);
        _entities.Clear();
        _lookup.GetLocalEntitiesIntersecting(grid, aabb, _entities, LookupFlags.Dynamic | LookupFlags.Static);
        foreach (var uid in _entities)
        {
            // don't care about non-physical entities
            if (!_fixturesQuery.TryComp(uid, out var fixtures))
                continue;

            foreach (var fixture in fixtures.Fixtures.Values)
            {
                if (!fixture.Hard)
                    continue;

                if ((fixture.CollisionLayer & mask) != 0)
                    return true;
            }
        }

        return false; // no entities colliding with the mask found
    }
}
