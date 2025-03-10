using Content.Shared._DV.Noospherics;

namespace Content.Server._DV.Singularity.Components;

[RegisterComponent]
public sealed partial class NoosphericFoodComponent : Component
{
    [DataField]
    public Dictionary<ParticleType, float> Particles = new()
    {
        { ParticleType.Delta, 1f },
        { ParticleType.Epsilon, 1f },
        { ParticleType.Omega, 1f },
        { ParticleType.Zeta, 1f },
    };
}
