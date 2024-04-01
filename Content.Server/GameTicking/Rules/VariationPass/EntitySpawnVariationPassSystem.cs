using System.Linq;
using Content.Server.GameTicking.Rules.VariationPass.Components;
using Content.Shared.Storage;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules.VariationPass;

/// <inheritdoc cref="EntitySpawnVariationPassComponent"/>
public sealed class EntitySpawnVariationPassSystem : VariationPassSystem<EntitySpawnVariationPassComponent>
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly EntityQuery<MapGridComponent> _mapgridQuery = default!;
    protected override void ApplyVariation(Entity<EntitySpawnVariationPassComponent> ent, ref StationVariationPassEvent args)
    {
        var largestStationGridUid = Stations.GetLargestGrid(args.Station);
        _mapgridQuery.TryGetComponent(largestStationGridUid, out var largestStationGridComponent);

        IEnumerable<Robust.Shared.Map.TileRef>? largestStationGridTiles = null;
        if (largestStationGridComponent is not null)
        {
            largestStationGridTiles = _map.GetAllTiles(args.Station, largestStationGridComponent);
        }
        else
        {
            return;
        }
        var totalTiles = largestStationGridTiles.Count();
        var dirtyMod = Random.NextGaussian(ent.Comp.TilesPerEntityAverage, ent.Comp.TilesPerEntityStdDev);
        var trashTiles = Math.Max((int) (totalTiles * (1 / dirtyMod)), 0);

        for (var i = 0; i < trashTiles; i++)
        {
            var curTileIndex = Random.Next(totalTiles);
            var curTileRef = largestStationGridTiles.ElementAt(curTileIndex);
            var coords = _map.GridTileToLocal(args.Station, largestStationGridComponent, curTileRef.GridIndices);

            var ents = EntitySpawnCollection.GetSpawns(ent.Comp.Entities, Random);
            foreach (var spawn in ents)
            {
                SpawnAtPosition(spawn, coords);
            }
        }
    }
}
