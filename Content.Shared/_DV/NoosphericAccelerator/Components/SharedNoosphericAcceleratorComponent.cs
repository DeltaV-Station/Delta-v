using Content.Shared._DV.Noospherics;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.NoosphericAccelerator.Components;

public enum NoosphericAcceleratorPowerLevel
{
    Standby,
    Level0,
    Level1,
    Level2,
    Level3,
}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorPowerState
{
    public bool Enabled = false;

    public Dictionary<ParticleType, NoosphericAcceleratorPowerLevel> ParticleStrengths = new()
    {
        { ParticleType.Delta, NoosphericAcceleratorPowerLevel.Standby },
        { ParticleType.Epsilon, NoosphericAcceleratorPowerLevel.Standby },
        { ParticleType.Omega, NoosphericAcceleratorPowerLevel.Standby },
        { ParticleType.Zeta, NoosphericAcceleratorPowerLevel.Standby },
    };
}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorSetEnableMessage : BoundUserInterfaceMessage
{
    public readonly bool Enabled;

    public NoosphericAcceleratorSetEnableMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorRescanPartsMessage : BoundUserInterfaceMessage
{
    public NoosphericAcceleratorRescanPartsMessage()
    {
    }
}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorSetPowerStateMessage(NoosphericAcceleratorPowerState state)
    : BoundUserInterfaceMessage
{
    public readonly NoosphericAcceleratorPowerState State = state;
}

[Serializable, NetSerializable]
public enum NoosphericAcceleratorWireStatus
{
    Power,
    Keyboard,
    Limiter,
    Strength,
}
