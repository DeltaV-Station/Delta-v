using Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Systems;
using Content.Shared.Destructible.Thresholds;

namespace Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Components;

/// <summary>
/// Raises or lowers glimmer when this artifact is triggered.
/// </summary>
[RegisterComponent, Access(typeof(XAEModifyGlimmerSystem))]
public sealed partial class XAEModifyGlimmerComponent : Component
{
    /// <summary>
    /// If glimmer is not in this range it won't do anything.
    /// Prevents the trigger being too extreme or too beneficial.
    /// </summary>
    [DataField(required: true)]
    public MinMax Range;

    /// <summary>
    /// Number to add to glimmer when triggering.
    /// </summary>
    [DataField(required: true)]
    public int Change;
}
