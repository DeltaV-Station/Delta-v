using Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Systems;

namespace Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Components;

/// <summary>
/// Makes people in a radius around it psionic when triggered.
/// </summary>
[RegisterComponent, Access(typeof(XAEPsionicInducerSystem))]
public sealed partial class XAEPsionicInducerComponent : Component
{
    /// <summary>
    /// Range to look for potential psionics in.
    /// </summary>
    [DataField(required: true)]
    public float Range;
}
