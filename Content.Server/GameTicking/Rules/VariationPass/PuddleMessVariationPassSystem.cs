using System.Linq;
using Content.Server.Fluids.EntitySystems;
using Content.Server.GameTicking.Rules.VariationPass.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Random.Helpers;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules.VariationPass;

/// <inheritdoc cref="PuddleMessVariationPassComponent"/>
public sealed class PuddleMessVariationPassSystem : VariationPassSystem<PuddleMessVariationPassComponent>
{
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly MapSystem _map = new();
    private EntityQuery<MapGridComponent> _mapgridQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mapgridQuery = GetEntityQuery<MapGridComponent>();
    }
    protected override void ApplyVariation(Entity<PuddleMessVariationPassComponent> ent, ref StationVariationPassEvent args)
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

        if (!_proto.TryIndex(ent.Comp.RandomPuddleSolutionFill, out var proto))
            return;

        var puddleMod = Random.NextGaussian(ent.Comp.TilesPerSpillAverage, ent.Comp.TilesPerSpillStdDev);
        var puddleTiles = Math.Max((int) (totalTiles * (1 / puddleMod)), 0);

        for (var i = 0; i < puddleTiles; i++)
        {
            var curTileIndex = Random.Next(totalTiles);
            var curTileRef = largestStationGridTiles.ElementAt(curTileIndex);
            var coords = _map.GridTileToLocal(args.Station, largestStationGridComponent, curTileRef.GridIndices);

            var sol = proto.Pick(Random);
            _puddle.TrySpillAt(coords, new Solution(sol.reagent, sol.quantity), out _, sound: false);
        }
    }
}
