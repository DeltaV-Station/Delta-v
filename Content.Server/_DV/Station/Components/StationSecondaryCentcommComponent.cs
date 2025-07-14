using Content.Server._DV.Station.Systems;
using Robust.Shared.Utility;

namespace Content.Server._DV.Station.Components;

/// <summary>
/// Spawns the non-emergency shuttle Central Command for a station on the same map as the <see cref="StationCentcommComponent"/>
/// </summary>
[RegisterComponent, Access(typeof(StationSecondaryCentcommSystem))]
public sealed partial class StationSecondaryCentcommComponent : Component
{
    /// <summary>
    /// The grid to load as secondary Central Command
    /// </summary>
    [DataField]
    public ResPath Path = new("/Maps/_DV/centcomm.yml");

    /// <summary>
    /// Minimum distance to load the grid at.
    /// </summary>
    [DataField]
    public float MinRange = 1000f;

    /// <summary>
    /// Maximum distance to load the grid at.
    /// </summary>
    [DataField]
    public float MaxRange = 1200f;
}
