using Robust.Shared.Serialization;

namespace Content.Shared._DV.NoosphericAccelerator.Components;

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorPowerState
{
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
public sealed class NoosphericAcceleratorSetPowerStateMessage(NoosphericAcceleratorPowerState state) : BoundUserInterfaceMessage
{
    public readonly NoosphericAcceleratorPowerState State = state;
}
