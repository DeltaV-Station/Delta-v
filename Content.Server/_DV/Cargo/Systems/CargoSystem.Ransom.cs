using Content.Server.Cargo.Components;
using Content.Server.Chat.Systems;
using Content.Server.Station.Components;
using Content.Shared._DV.Traitor;
using Content.Shared.Bed.Sleep;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Database;
using Content.Shared.StatusEffect;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Cargo.Systems;

/// <summary>
/// Handles purchasing ransomed entities from a cargo request console.
/// </summary>
public sealed partial class CargoSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    /// <summary>
    /// The crate to put ransomed entities in when purchasing them.
    /// </summary>
    public static readonly EntProtoId RansomCrate = "CrateSyndicate";

    /// <summary>
    /// Status effect for <see cref="ForcedSleepingComponent"/>.
    /// </summary>
    public static readonly ProtoId<StatusEffectPrototype> StatusEffectKey = "ForcedSleep";

    /// <summary>
    /// Sound to play for the ransom victim when being "trafficked" to the ATS.
    /// </summary>
    public static readonly SoundSpecifier HypoSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");

    /// <summary>
    /// How long to be slept for.
    /// </summary>
    public static readonly TimeSpan SleepyTime = TimeSpan.FromSeconds(10);

    private void InitializeRansom()
    {
        Subs.BuiEvents<CargoOrderConsoleComponent>(CargoConsoleUiKey.Orders, subs =>
        {
            subs.Event<RansomPurchaseMessage>(OnPurchaseMessage);
        });
    }

    private void OnPurchaseMessage(Entity<CargoOrderConsoleComponent> ent, ref RansomPurchaseMessage args)
    {
        var user = args.Actor;
        if (!_accessReaderSystem.IsAllowed(user, ent))
        {
            ConsolePopup(user, Loc.GetString("cargo-console-order-not-allowed"));
            PlayDenySound(ent, ent);
            return;
        }

        // malf client or they somehow got gibbed in jail
        if (GetEntity(args.Entity) is not { Valid: true } uid ||
            // got released already
            !TryComp<RansomComponent>(uid, out var ransom) ||
            // not on a station
            _station.GetOwningStation(uid) is not {} station ||
            !TryComp<StationBankAccountComponent>(station, out var bank) ||
            !TryComp<StationDataComponent>(station, out var stationData))
        {
            ConsolePopup(user, Loc.GetString("cargo-console-station-not-found"));
            PlayDenySound(ent, ent);
            return;
        }

        var cost = ransom.Ransom;
        var balance = bank.Accounts[bank.PrimaryAccount];
        if (cost > balance)
        {
            ConsolePopup(user, Loc.GetString("cargo-console-insufficient-funds", ("cost", cost)));
            PlayDenySound(ent, ent);
            return;
        }

        // paid the ransom, time to bring em home
        if (TryReturnEntity(uid, stationData) is not {} trade)
        {
            ConsolePopup(user, Loc.GetString("cargo-console-unfulfilled"));
            PlayDenySound(ent, ent);
            return;
        }

        _audio.PlayPvs(ApproveSound, ent);
        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(user):user} paid the ransom of ${cost} for {ToPrettyString(uid)} with balance at {balance}");

        UpdateBankAccount((station, bank), -cost, CreateAccountDistribution((station, bank)));

        // announce it so everyone knows
        var msg = Loc.GetString("syndicate-ransom-return-announcement", ("station", trade));
        var sender = Loc.GetString("syndicate-ransom-return-announcement-sender");
        var sound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");
        var color = Color.Red;
        _chat.DispatchGlobalAnnouncement(msg, sender, playSound: true, sound, color);
    }

    // like TryFulfillOrder but for ransoms
    private EntityUid? TryReturnEntity(EntityUid uid, StationDataComponent station)
    {
        _listEnts.Clear();
        GetTradeStations(station, ref _listEnts);
        // Try to fulfill from any station where possible, if the pad is not occupied.
        foreach (var trade in _listEnts)
        {
            var tradePads = GetCargoPallets(trade, BuySellType.Buy);
            _random.Shuffle(tradePads);

            var freePads = GetFreeCargoPallets(trade, tradePads);
            if (freePads.Count == 0)
                continue;

            // sleepy time
            _audio.PlayPvs(HypoSound, uid);
            _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(uid, StatusEffectKey, SleepyTime, refresh: false);

            var pad = _random.Pick(freePads);
            var coordinates = new EntityCoordinates(trade, pad.Transform.LocalPosition);
            var crate = Spawn(RansomCrate, coordinates);
            if (!_entityStorage.Insert(uid, crate))
                _transformSystem.DropNextTo(uid, crate); // just teleport directly if it somehow fails
            return trade;
        }

        return null;
    }
}
