using System.Threading.Tasks;
using Content.Shared.Procedural;
using Content.Shared.Procedural.PostGeneration;
using Content.Shared.Storage;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    /// <summary>
    /// <see cref="CornerClutterDunGen"/>
    /// </summary>
    private async Task PostGen(CornerClutterDunGen gen, DungeonData data, Dungeon dungeon, HashSet<Vector2i> reservedTiles, Random random)
    {
        if (!data.SpawnGroups.TryGetValue(DungeonDataKey.CornerClutter, out var corner))
        {
            _sawmill.Error(Environment.StackTrace);
            return;
        }

        foreach (var tile in dungeon.CorridorTiles)
        {
<<<<<<< HEAD:Content.Server/Procedural/DungeonJob/DungeonJob.PostGenCornerClutter.cs
            var blocked = _anchorable.TileFree(_grid, tile, DungeonSystem.CollisionLayer, DungeonSystem.CollisionMask);
=======
            if (reservedTiles.Contains(tile))
                continue;

            var blocked = _anchorable.TileFree((_gridUid, _grid), tile, DungeonSystem.CollisionLayer, DungeonSystem.CollisionMask);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30:Content.Server/Procedural/DungeonJob/DungeonJob.CornerClutter.cs

            if (blocked)
                continue;

            // If at least 2 adjacent tiles are blocked consider it a corner
            for (var i = 0; i < 4; i++)
            {
                var dir = (Direction) (i * 2);
                blocked = HasWall(tile + dir.ToIntVec());

                if (!blocked)
                    continue;

                var nextDir = (Direction) ((i + 1) * 2 % 8);
                blocked = HasWall(tile + nextDir.ToIntVec());

                if (!blocked)
                    continue;

                if (random.Prob(gen.Chance))
                {
                    var coords = _maps.GridTileToLocal(_gridUid, _grid, tile);
<<<<<<< HEAD:Content.Server/Procedural/DungeonJob/DungeonJob.PostGenCornerClutter.cs
                    var protos = EntitySpawnCollection.GetSpawns(_prototype.Index(corner).Entries, random);
                    _entManager.SpawnEntities(coords, protos);
=======
                    var protos = _entTable.GetSpawns(contentsTable, random);
                    _entManager.SpawnEntitiesAttachedTo(coords, protos);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30:Content.Server/Procedural/DungeonJob/DungeonJob.CornerClutter.cs
                }

                break;
            }
        }
    }
}
