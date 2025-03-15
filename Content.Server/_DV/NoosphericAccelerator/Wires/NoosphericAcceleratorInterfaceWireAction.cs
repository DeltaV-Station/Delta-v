using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Server._DV.NoosphericAccelerator.EntitySystems;
using Content.Server.Wires;
using Content.Shared._DV.NoospericAccelerator.Components;
using Content.Shared.Wires;

namespace Content.Server._DV.NoosphericAccelerator.Wires;

public sealed partial class
    NoosphericAcceleratorKeyboardWireAction : ComponentWireAction<NoosphericAcceleratorControlBoxComponent>
{
    public override string Name { get; set; } = "wire-name-pa-keyboard";
    public override Color Color { get; set; } = Color.LimeGreen;
    public override object StatusKey { get; } = NoosphericAcceleratorWireStatus.Keyboard;

    public override StatusLightState? GetLightState(Wire wire, NoosphericAcceleratorControlBoxComponent component)
    {
        return component.InterfaceDisabled ? StatusLightState.BlinkingFast : StatusLightState.On;
    }

    public override bool Cut(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        controller.InterfaceDisabled = true;
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();
        paSystem.UpdateUI(wire.Owner, controller);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        controller.InterfaceDisabled = false;
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();
        paSystem.UpdateUI(wire.Owner, controller);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        controller.InterfaceDisabled = !controller.InterfaceDisabled;
    }
}
