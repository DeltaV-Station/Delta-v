using Content.Shared._DV.Noospherics;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.NoosphericAccelerator.Components;

using ParticlePowerDict = Dictionary<ParticleType, NoosphericAcceleratorPowerLevel>;

[NetSerializable, Serializable]
public enum NoosphericAcceleratorVisuals
{
    VisualState
}

[NetSerializable, Serializable]
public enum NoosphericAcceleratorVisualState
{
    //Open, //no prefix
    //Wired, //w prefix
    Unpowered, //c prefix
    Powered //p prefix
}

public enum NoosphericAcceleratorPowerLevel : byte
{
    Standby = 0,
    Level0,
    Level1,
    Level2,
    Level3,
}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorPowerState
{
    public NoosphericAcceleratorPowerState()
    {
        Particles = Standby();
    }

    public NoosphericAcceleratorPowerState(ParticlePowerDict strengths)
    {
        Particles = strengths;
    }

    public ParticlePowerDict Particles;

    public static ParticlePowerDict Standby()
    {
        return new()
        {
            { ParticleType.Delta, NoosphericAcceleratorPowerLevel.Standby },
            { ParticleType.Epsilon, NoosphericAcceleratorPowerLevel.Standby },
            { ParticleType.Omega, NoosphericAcceleratorPowerLevel.Standby },
            { ParticleType.Zeta, NoosphericAcceleratorPowerLevel.Standby },
        };
    }

    public float AveragePower()
    {
        var aggregate = 0f;
        var enumValues = Enum.GetValues<ParticleType>();
        foreach (var type in enumValues)
        {
            aggregate += (float)Particles[type];
        }

        return aggregate / enumValues.Length;
    }
}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorSetEnableMessage(bool enabled) : BoundUserInterfaceMessage
{
    public readonly bool Enabled = enabled;
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

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorUIState(
    bool assembled,
    bool enabled,
    NoosphericAcceleratorPowerState state,
    int powerReceive,
    int powerDraw,
    bool emitterStarboardExists,
    bool emitterForeExists,
    bool emitterPortExists,
    bool powerBoxExists,
    bool fuelChamberExists,
    bool endCapExists,
    bool interfaceBlock,
    NoosphericAcceleratorPowerLevel maxLevel,
    bool wirePowerBlock) : BoundUserInterfaceState
{
    public bool Assembled = assembled;
    public bool Enabled = enabled;
    public NoosphericAcceleratorPowerState State = state;
    public int PowerDraw = powerDraw;
    public int PowerReceive = powerReceive;

    //dont need a bool for the controlbox because... this is sent to the controlbox :D
    public bool EmitterStarboardExists = emitterStarboardExists;
    public bool EmitterForeExists = emitterForeExists;
    public bool EmitterPortExists = emitterPortExists;
    public bool PowerBoxExists = powerBoxExists;
    public bool FuelChamberExists = fuelChamberExists;
    public bool EndCapExists = endCapExists;

    public bool InterfaceBlock = interfaceBlock;
    public NoosphericAcceleratorPowerLevel MaxLevel = maxLevel;
    public bool WirePowerBlock = wirePowerBlock;
}

[NetSerializable, Serializable]
public enum NoosphericAcceleratorControlBoxUiKey
{
    Key
}
