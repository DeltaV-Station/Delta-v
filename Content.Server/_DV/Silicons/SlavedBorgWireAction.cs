using Content.Server._DV.Silicons.Laws;
using Content.Server.Wires;
using Content.Shared.Doors;
using Content.Shared._DV.Silicons.Laws;
using Content.Shared._DV.Silicons;
using Content.Shared.Wires;

namespace Content.Server._DV.Silicons;

/// <summary>
/// Controls whether the cyborg's transponder is active
/// </summary>
public sealed partial class SlavedBorgWireAction : ComponentWireAction<SlavedBorgComponent>
{
    public override string Name { get; set; } = "wire-name-slaved-borg";
    public override Color Color { get; set; } = Color.Coral;
    public override object StatusKey => BorgWireActionKey.SlavedKey;

    public override StatusLightState? GetLightState(Wire wire, SlavedBorgComponent component)
    {
        return component.ShouldBeAdded ? StatusLightState.On : StatusLightState.Off;
    }

    public override bool Cut(EntityUid user, Wire wire, SlavedBorgComponent component)
    {
        EntityManager.System<SlavedBorgSystem>()
            .SetShouldBeAdded((wire.Owner, component), false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, SlavedBorgComponent component)
    {
        EntityManager.System<SlavedBorgSystem>()
            .SetShouldBeAdded((wire.Owner, component), true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, SlavedBorgComponent component)
    {
    }
}
