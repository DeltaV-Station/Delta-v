using Content.Server.Silicons.Borgs;
using Content.Server.Wires;
using Content.Shared._DV.Silicons;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Wires;

namespace Content.Server._DV.Silicons;

/// <summary>
/// Controls whether the cyborg's transponder is active
/// </summary>
public sealed partial class BorgTransponderWireAction : ComponentWireAction<BorgTransponderComponent>
{
    public override string Name { get; set; } = "wire-name-borg-transponder";
    public override Color Color { get; set; } = Color.YellowGreen;
    public override object StatusKey => BorgWireActionKey.TransponderKey;

    public override StatusLightState? GetLightState(Wire wire, BorgTransponderComponent component)
    {
        return component.Active ? StatusLightState.On : StatusLightState.Off;
    }

    public override bool Cut(EntityUid user, Wire wire, BorgTransponderComponent component)
    {
        EntityManager.System<BorgSystem>()
            .SetTransponderActive((wire.Owner, component), false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, BorgTransponderComponent component)
    {
        EntityManager.System<BorgSystem>()
            .SetTransponderActive((wire.Owner, component), true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, BorgTransponderComponent component)
    {
    }
}
