using Content.Server._DV.Station.Systems;
using Content.Shared._DV.Planet;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DV.Station.Components;

/// <summary>
/// Loads a planet map on mapinit and spawns a grid on it (e.g. a mining base).
/// The map can then be FTLd to by any shuttle matching its whitelist.
/// </summary>
[RegisterComponent, Access(typeof(StationPlanetSpawnerSystem))]
public sealed partial class StationPlanetSpawnerComponent : Component
{
    /// <summary>
    /// The planet to create.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PlanetPrototype> Planet;

    /// <summary>
    /// Path to the grid to load onto the map.
    /// </summary>
    [DataField(required: true)]
    public ResPath? GridPath;

    /// <summary>
    /// The map that was loaded.
    /// </summary>
    [DataField]
    public EntityUid? Map;
}
