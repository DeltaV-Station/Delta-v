using Robust.Shared.Serialization;

// TODO: Move this shit to the right place
namespace Content.Shared._DV.Singularity.Components;

[NetSerializable, Serializable]
public sealed class NoosphereAcceleratorPowerState
{

}

[NetSerializable, Serializable]
public sealed class NoosphericAcceleratorSetPowerStateMessage(NoosphereAcceleratorPowerState state) : BoundUserInterfaceMessage
{
    public readonly NoosphereAcceleratorPowerState State = state;
}
