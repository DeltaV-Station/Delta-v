using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Server._DV.NoosphericAccelerator.EntitySystems;
using Content.Server.Wires;
using Content.Shared._DV.NoospericAccelerator.Components;
using Content.Shared.Wires;

namespace Content.Server._DV.NoosphericAccelerator.Wires;

public sealed partial class
    NoosphericAcceleratorStrengthWireAction : ComponentWireAction<NoosphericAcceleratorControlBoxComponent>
{
    public override string Name { get; set; } = "wire-name-pa-strength";
    public override Color Color { get; set; } = Color.Blue;
    public override object StatusKey { get; } = NoosphericAcceleratorWireStatus.Strength;

    public override StatusLightState? GetLightState(Wire wire, NoosphericAcceleratorControlBoxComponent component)
    {
        return component.StrengthLocked ? StatusLightState.BlinkingSlow : StatusLightState.On;
    }

    public override bool Cut(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        controller.StrengthLocked = true;
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();
        paSystem.UpdateUI(wire.Owner, controller);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        controller.StrengthLocked = false;
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();
        paSystem.UpdateUI(wire.Owner, controller);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, NoosphericAcceleratorControlBoxComponent controller)
    {
        var paSystem = EntityManager.System<NoosphericAcceleratorSystem>();
        paSystem.SetStrength(wire.Owner,
            (NoosphericAcceleratorPowerState)((int)controller.SelectedStrength + 1),
            user,
            controller);
    }
}
