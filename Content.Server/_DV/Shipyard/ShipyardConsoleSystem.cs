using Content.Server.Cargo.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.Popups;
using Content.Shared.Shipyard;
using Content.Shared.Shipyard.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server.Shipyard;

public sealed class ShipyardConsoleSystem : SharedShipyardConsoleSystem
{
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly ShipyardSystem _shipyard = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BankBalanceUpdatedEvent>(OnBalanceUpdated);
        Subs.BuiEvents<ShipyardConsoleComponent>(ShipyardConsoleUiKey.Key,
            subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
        });
    }

    protected override void TryPurchase(Entity<ShipyardConsoleComponent> ent, EntityUid user, VesselPrototype vessel)
    {
        // client prevents asking for this so dont need feedback for validation
        if (_whitelist.IsWhitelistFail(vessel.Whitelist, ent))
            return;

        var purchasingGrid = Transform(ent).GridUid;
        Entity<StationBankAccountComponent>? bankAccount = null;
        if (ent.Comp.UseStationFunds)
        {
            bankAccount = GetBankAccount(ent);
            if (!bankAccount.HasValue)
            {
                var popup = Loc.GetString("shipyard-console-error-bank");
                Popup.PopupEntity(popup, ent, user, PopupType.SmallCaution);
                Audio.PlayPvs(ent.Comp.DenySound, ent);
                return;
            }

            if (bankAccount.Value.Comp.Accounts[bankAccount.Value.Comp.PrimaryAccount] < vessel.Price)
            {
                var popup = Loc.GetString("cargo-console-insufficient-funds", ("cost", vessel.Price));
                Popup.PopupEntity(popup, ent, user, PopupType.SmallCaution);
                Audio.PlayPvs(ent.Comp.DenySound, ent);
                return;
            }

            purchasingGrid = bankAccount.Value.Owner;
        }

        if (purchasingGrid is not { } grid
            || !_shipyard.TrySendShuttle(grid, vessel.Path, out var shuttle))
        {
            var popup = Loc.GetString("shipyard-console-error");
            Popup.PopupEntity(popup, ent, user, PopupType.SmallCaution);
            Audio.PlayPvs(ent.Comp.DenySound, ent);
            return;
        }

        if (bankAccount.HasValue)
            _cargo.UpdateBankAccount(bankAccount.Value.Owner, -vessel.Price, _cargo.CreateAccountDistribution(bankAccount.Value));

        _meta.SetEntityName(shuttle.Value, $"{vessel.Name} {_random.Next(1000):000}");
        var message = Loc.GetString("shipyard-console-docking", ("vessel", vessel.Name));
        _radio.SendRadioMessage(ent, message, ent.Comp.Channel, ent);
        Audio.PlayPvs(ent.Comp.ConfirmSound, ent);
    }

    private void OnBalanceUpdated(ref BankBalanceUpdatedEvent args)
    {
        var query = EntityQueryEnumerator<ShipyardConsoleComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_ui.IsUiOpen(uid, ShipyardConsoleUiKey.Key))
                return;

            if (GetBankAccount(uid) is { } bank)
                UpdateUI(uid, args.Balance[bank.Comp.PrimaryAccount]);
        }
    }

    private void OnOpened(Entity<ShipyardConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUI(ent);
    }

    private void UpdateUI(EntityUid uid)
    {
        if (GetBankAccount(uid) is { } bank)
            UpdateUI(uid, bank.Comp.Accounts[bank.Comp.PrimaryAccount]);
    }

    private void UpdateUI(EntityUid uid, int balance)
    {
        if (!_shipyard.Enabled)
            return;

        var state = new ShipyardConsoleState(balance);
        _ui.SetUiState(uid, ShipyardConsoleUiKey.Key, state);
    }

    private Entity<StationBankAccountComponent>? GetBankAccount(EntityUid console)
    {
        if (_station.GetOwningStation(console) is not { } station)
            return null;

        if (!TryComp<StationBankAccountComponent>(station, out var bank))
            return null;

        return (station, bank);
    }
}
