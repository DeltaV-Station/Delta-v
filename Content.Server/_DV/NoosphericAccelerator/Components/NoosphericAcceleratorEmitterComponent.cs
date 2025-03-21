using Robust.Shared.Prototypes;

namespace Content.Server._DV.NoosphericAccelerator.Components;

[RegisterComponent]
public sealed partial class NoosphericAcceleratorEmitterComponent : Component
{
    [DataField]
    public EntProtoId EmittedPrototype = "ParticlesProjectile";

    [DataField("emitterType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public NoosphericAcceleratorEmitterType Type = NoosphericAcceleratorEmitterType.Fore;

    public override string ToString()
    {
        return base.ToString() + $" EmitterType:{Type}";
    }
}

public enum NoosphericAcceleratorEmitterType
{
    Port,
    Fore,
    Starboard
}
