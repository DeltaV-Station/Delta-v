using Robust.Shared.GameStates;

namespace Content.Shared._DV.Medical.CrewMonitoring;

/// <summary>
/// Marker component that makes a crew monitoring console focus on
/// a station in the same map, instead of the grid the console is on.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LongRangeCrewMonitorComponent : Component;
