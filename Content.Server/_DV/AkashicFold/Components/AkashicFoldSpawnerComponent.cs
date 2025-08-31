using Content.Server._DV.Station.Systems;
using Content.Shared._DV.Planet;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DV.AkashicFold.Components;

[RegisterComponent, Access(typeof(AkashicFoldSpawnerSystem))]
public sealed partial class AkashicFoldSpawnerComponent : Component
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
