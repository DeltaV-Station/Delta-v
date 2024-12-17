using Content.Server.Station.Systems;
using Robust.Shared.Utility;

namespace Content.Server.Station.Components;

/// <summary>
/// Loads a surface map on mapinit.
/// </summary>
[RegisterComponent, Access(typeof(StationSurfaceSystem))]
public sealed partial class StationSurfaceComponent : Component
{
    /// <summary>
    /// Path to the map to load.
    /// </summary>
    [DataField(required: true)]
    public ResPath? MapPath;

    /// <summary>
    /// The map that was loaded.
    /// </summary>
    [DataField]
    public EntityUid? Map;
}
