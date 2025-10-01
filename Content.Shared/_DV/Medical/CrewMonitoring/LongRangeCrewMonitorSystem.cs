using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._DV.Medical.CrewMonitoring;

public sealed class LongRangeCrewMonitorSystem : EntitySystem
{
    /// <summary>
    /// Finds the largest (presumably the main station) grid on the same map as the argument.
    /// </summary>
    /// <param name="map"></param>
    /// <returns>Returns null if not found</returns>
    public EntityUid? FindLargestStationGridInMap(MapId map)
    {
        // also requiring MapGrid incase StationMember gets used for non-grids in the future
        (EntityUid?, int) biggest_grid = (null, 0);
        var query = EntityQueryEnumerator<StationMemberComponent, MapGridComponent>();
        while (query.MoveNext(out var grid, out _, out var mapgrid))
        {
            if (Transform(grid).MapID == map && mapgrid.ChunkCount > biggest_grid.Item2)
                biggest_grid = (grid, mapgrid.ChunkCount);
        }
        return biggest_grid.Item1;
    }
}
