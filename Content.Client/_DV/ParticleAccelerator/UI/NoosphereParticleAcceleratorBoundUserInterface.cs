using Content.Shared.Singularity.Components;
using Robust.Client.UserInterface;

namespace Content.Client._DV.ParticleAccelerator.UI;
public sealed class NoosphereParticleAcceleratorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NoosphereParticleAcceleratorControlMenu? _menu;

    public NoosphereParticleAcceleratorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<NoosphereParticleAcceleratorControlMenu>();
    }

    public void SendEnableMessage(bool enable)
    {
        SendMessage(new ParticleAcceleratorSetEnableMessage(enable));
    }

    public void SendPowerStateMessage(ParticleAcceleratorPowerState state)
    {
        SendMessage(new ParticleAcceleratorSetPowerStateMessage(state));
    }

    public void SendScanPartsMessage()
    {
        SendMessage(new ParticleAcceleratorRescanPartsMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
    }
}
