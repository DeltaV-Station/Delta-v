using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Server._DV.NoosphericAccelerator.EntitySystems;
using Content.Server.Wires;
using Content.Shared._DV.NoospericAccelerator.Components;
using Content.Shared.Wires;

namespace Content.Server._DV.NoosphericAccelerator.Wires;

public sealed partial class
    NoosphericAcceleratorPowerWireAction : ComponentWireAction<NoosphericAcceleratorControlBoxComponent>
{
    public override string Name { get; set; } = "wire-name-pa-power";
    public override Color Color { get; set; } = Color.Yellow;
    public override object StatusKey { get; } = NoosphericAcceleratorWireStatus.Power;

    public override StatusLightState? GetLightState(Wire wire, NoosphericAcceleratorControlBoxComponent component)
    {
        if (!component.CanBeEnabled)
            return StatusLightState.Off;
        return component.Enabled ? StatusLightState.On : StatusLightState.BlinkingSlow;
    }

    public override bool Cut(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();

        controller.CanBeEnabled = false;
        paSystem.SwitchOff(wire.Owner, user, controller);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        controller.CanBeEnabled = true;
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();

        if (controller.Enabled)
            paSystem.SwitchOff(wire.Owner, user, controller);
        else if (controller.Assembled)
            paSystem.SwitchOn(wire.Owner, user, controller);
    }
}
