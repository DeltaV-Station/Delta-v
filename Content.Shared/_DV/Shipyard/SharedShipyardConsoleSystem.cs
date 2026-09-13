using Content.Shared.Access.Systems;
using Content.Shared.Popups;
using Content.Shared.Shipyard.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Shipyard;

/// <summary>
/// Handles shipyard console interaction.
/// <c>ShipyardSystem</c> does the heavy lifting serverside.
/// </summary>
public abstract class SharedShipyardConsoleSystem : EntitySystem
{
    [Dependency] private AccessReaderSystem _access = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] protected SharedAudioSystem Audio = default!;
    [Dependency] protected SharedPopupSystem Popup = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<ShipyardConsoleComponent>(ShipyardConsoleUiKey.Key,
            subs =>
        {
            subs.Event<ShipyardConsolePurchaseMessage>(OnPurchase);
        });
    }

    private void OnPurchase(Entity<ShipyardConsoleComponent> ent, ref ShipyardConsolePurchaseMessage msg)
    {
        var user = msg.Actor;
        if (!_access.IsAllowed(user, ent.Owner))
        {
            Popup.PopupClient(Loc.GetString("comms-console-permission-denied"), ent, user);
            Audio.PlayPredicted(ent.Comp.DenySound, ent, user);
            return;
        }

        if (!_proto.TryIndex(msg.Vessel, out var vessel) || _whitelistSystem.IsWhitelistFail(vessel.Whitelist, ent))
            return;

        TryPurchase(ent, user, vessel);
    }

    protected virtual void TryPurchase(Entity<ShipyardConsoleComponent> ent, EntityUid user, VesselPrototype vessel)
    {
    }
}
