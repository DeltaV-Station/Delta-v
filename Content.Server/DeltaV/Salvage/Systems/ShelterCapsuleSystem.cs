using Content.Server.Procedural;
using Content.Shared.DeltaV.Salvage.Components;
using Content.Shared.DeltaV.Salvage.Systems;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Server.DeltaV.Salvage.Systems;

public sealed class ShelterCapsuleSystem : SharedShelterCapsuleSystem
{
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private HashSet<Entity<TransformComponent>> _entities = new();

    protected override LocId? TrySpawnRoom(Entity<ShelterCapsuleComponent> ent)
    {
        var xform = Transform(ent);
        if (xform.GridUid is not {} gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return "shelter-capsule-error-space";

        var gridXform = Transform(gridUid);
        var center = _map.LocalToTile(gridUid, grid, xform.Coordinates);
        var room = _proto.Index(ent.Comp.Room);
        var origin = center - room.Size / 2;

        // check that every tile it needs isn't blocked
        var mask = CollisionGroup.MobMask;
        for (int y = 0; y < room.Size.Y; y++)
        {
            for (int x = 0; x < room.Size.X; x++)
            {
                var pos = origin + new Vector2i(x, y);
                var tile = _map.GetTileRef((gridUid, grid), pos);
                if (tile.Tile.IsEmpty || _turf.IsTileBlocked(gridUid, pos, mask, grid, gridXform))
                    return "shelter-capsule-error-obstructed";
            }
        }

        _dungeon.SpawnRoom(gridUid,
            grid,
            origin,
            room,
            new Random(),
            null);

        QueueDel(ent);
        return null;
    }
}
