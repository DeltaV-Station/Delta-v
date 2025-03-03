using Content.Shared.Singularity.Components;

namespace Content.Server._DV.NoosphericAccelerator.Components;

[RegisterComponent]
public sealed partial class NoosphericParticleProjectileComponent : Component
{
    public NoosphericAcceleratorPowerState State;
}
