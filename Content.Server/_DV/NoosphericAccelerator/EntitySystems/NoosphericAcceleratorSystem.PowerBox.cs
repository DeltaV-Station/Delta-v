using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Server.Power.EntitySystems;

namespace Content.Server._DV.NoosphericAccelerator.EntitySystems;

public sealed partial class NoosphericAcceleratorSystem
{
    private void InitializePowerBoxSystem()
    {
        SubscribeLocalEvent<NoosphericAcceleratorPowerBoxComponent, PowerConsumerReceivedChanged>(
            PowerBoxReceivedChanged);
    }

    private void PowerBoxReceivedChanged(
        Entity<NoosphericAcceleratorPowerBoxComponent> ent,
        ref PowerConsumerReceivedChanged args)
    {
        if (!TryComp<NoosphericAcceleratorPartComponent>(ent, out var part))
            return;
        if (!TryComp<NoosphericAcceleratorControlBoxComponent>(part.Master, out var controller))
            return;

        var master = part.Master!.Value;
        if (controller.Enabled && args.ReceivedPower >=
            args.DrawRate * NoosphericAcceleratorControlBoxComponent.RequiredPowerRatio)
            PowerOn(master, comp: controller);
        else
            PowerOff(master, comp: controller);

        UpdateUI(master, controller);
    }
}
