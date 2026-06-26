using Content.Server.Wires;
using Content.Shared._DV.Silicons.Borgs;
using Content.Shared._DV.Silicons;
using Content.Shared.Wires;

namespace Content.Server._DV.Silicons;

/// <summary>
/// Controls whether the cyborg's ID chip slot is active
/// </summary>
public sealed partial class BorgIdChipWireAction : ComponentWireAction<IdChipSlotComponent>
{
    public override string Name { get; set; } = "wire-name-borg-id-chip";
    public override Color Color { get; set; } = Color.Thistle;
    public override object StatusKey => BorgWireActionKey.ChipKey;

    public override StatusLightState? GetLightState(Wire wire, IdChipSlotComponent component)
    {
        return component.Active ? StatusLightState.On : StatusLightState.Off;
    }

    public override bool Cut(EntityUid user, Wire wire, IdChipSlotComponent component)
    {
        EntityManager.System<IdChipSlotSystem>()
            .SetActive((wire.Owner, component), false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, IdChipSlotComponent component)
    {
        EntityManager.System<IdChipSlotSystem>()
            .SetActive((wire.Owner, component), true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, IdChipSlotComponent component)
    {
    }
}
