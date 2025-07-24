using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._DV.Medical.CrewMonitoring;

public sealed class LongRangeCrewMonitorSystem : EntitySystem
{
    /// <summary>
    /// Finds an arbitrary station grid on the same map as the argument.
    /// Returns null if no grid was found.
    /// </summary>
    public EntityUid? FindStationGridInMap(MapId map)
    {
        // also requiring MapGrid incase StationMember gets used for non-grids in the future
        var query = EntityQueryEnumerator<StationMemberComponent, MapGridComponent>();
        while (query.MoveNext(out var grid, out _, out _))
        {
            if (Transform(grid).MapID == map)
                return grid;
        }

        return null;
    }
}
