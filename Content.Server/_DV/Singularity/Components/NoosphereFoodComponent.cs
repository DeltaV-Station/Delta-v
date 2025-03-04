namespace Content.Server._DV.Singularity.Components;

[RegisterComponent]
public sealed partial class NoosphereFoodComponent : Component
{
    // TODO: Make this into a cool hash map? Or just leave as-is...
    [DataField]
    public float DeltaParticles = 1f;
    [DataField]
    public float EpsilonParticles = 1f;
    [DataField]
    public float ZetaParticles = 1f;
    [DataField]
    public float OmegaParticles = 1f;
}
