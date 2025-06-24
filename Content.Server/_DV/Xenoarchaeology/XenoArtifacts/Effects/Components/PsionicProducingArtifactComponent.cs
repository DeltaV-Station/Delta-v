using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Systems;

namespace Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Makes people in a radius around it psionic when triggered.
/// </summary>
[RegisterComponent, Access(typeof(PsionicProducingArtifactSystem))]
public sealed partial class PsionicProducingArtifactComponent : Component
{
    /// <summary>
    /// Range to look for potential psionics in.
    /// </summary>
    [DataField(required: true)]
    public float Range;

    /// <summary>
    /// Number of times this node can trigger before it switches to doing nothing.
    /// </summary>
    [DataField]
    public int Limit = 1;
}
