using Content.Server.Wires;
using Content.Shared._DV.Silicons;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Wires;

namespace Content.Server._DV.Silicons;

/// <summary>
/// Controls whether the power cell slot is active
/// </summary>
public sealed partial class PowerCellSlotWireAction : ComponentWireAction<PowerCellSlotComponent>
{
    public override string Name { get; set; } = "wire-name-power-cell";
    public override Color Color { get; set; } = Color.BurlyWood;
    public override object StatusKey => BorgWireActionKey.CellKey;

    public override StatusLightState? GetLightState(Wire wire, PowerCellSlotComponent component)
    {
        return component.Active ? StatusLightState.On : StatusLightState.Off;
    }

    public override bool Cut(EntityUid user, Wire wire, PowerCellSlotComponent component)
    {
        EntityManager.System<PowerCellSystem>()
            .SetSlotActive((wire.Owner, component), false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, PowerCellSlotComponent component)
    {
        EntityManager.System<PowerCellSystem>()
            .SetSlotActive((wire.Owner, component), true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, PowerCellSlotComponent component)
    {
    }
}
