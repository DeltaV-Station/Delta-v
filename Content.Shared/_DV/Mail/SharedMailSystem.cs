using System.Linq;
using Content.Shared._DV.Cargo.Components;
using Content.Shared._DV.Cargo.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Objectives.Components;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Content.Shared.Station;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Mail;

public abstract class SharedMailSystem : EntitySystem
{
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly LogisticStatsSystem _logisticsStats = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly SharedCargoSystem _cargo = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> RecyclableTag = "Recyclable";
    private static readonly ProtoId<TagPrototype> TrashTag = "Trash";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<MailComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<MailComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MailComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MailComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<MailComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MailComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<MailComponent, UseInHandEvent>(OnUseInHand, before: new[] { typeof(FoodSystem), typeof(IngestionSystem) });
    }

    private static void OnShutdown(EntityUid uid, MailComponent component, ComponentShutdown args)
    {
        component.PriorityCancelToken?.Cancel();
    }

    /// <summary>
    /// Handle the <see cref="AfterInteractEvent"/> by checking the ID against the mail.
    /// </summary>
    private void OnAfterInteractUsing(EntityUid uid, MailComponent component, ref AfterInteractUsingEvent args)
    {
        if (!args.CanReach || !component.IsLocked)
            return;

        if (!HasComp<AccessReaderComponent>(uid))
            return;

        IdCardComponent? idCard = null; // We need an ID card.

        if (HasComp<PdaComponent>(args.Used)) // Can we find it in a PDA if the user is using that?
        {
            _idCard.TryGetIdCard(args.Used, out var pdaId);
            idCard = pdaId;
        }

        if (idCard == null &&
            HasComp<IdCardComponent>(args.Used)) // If we still don't have an ID, check if the item itself is one
            idCard = Comp<IdCardComponent>(args.Used);

        if (idCard == null) // Return if we still haven't found an id card.
            return;

        if (!HasComp<EmaggedComponent>(uid))
        {
            if (idCard.FullName != component.Recipient || idCard.LocalizedJobTitle != component.RecipientJob)
            {
                _popup.PopupPredicted(Loc.GetString("mail-recipient-mismatch"), uid, args.User);
                return;
            }

            if (!_access.IsAllowed(uid, args.User))
            {
                _popup.PopupPredicted(Loc.GetString("mail-invalid-access"), uid, args.User);
                return;
            }
        }

        // DeltaV - Add earnings to logistic stats
        ExecuteForEachLogisticsStats(uid,
            (station, logisticStats) =>
            {
                _logisticsStats.AddOpenedMailEarnings(station,
                    logisticStats,
                    component.IsProfitable ? component.Bounty : 0);
            });

        UnlockMail(uid, component);

        if (!component.IsProfitable)
        {
            _popup.PopupPredicted(Loc.GetString("mail-unlocked"), uid, args.User);
            return;
        }

        _popup.PopupPredicted(Loc.GetString("mail-unlocked-reward", ("bounty", component.Bounty)), uid, args.User);
        component.IsProfitable = false;

        var query = EntityQueryEnumerator<StationBankAccountComponent>();
        while (query.MoveNext(out var station, out var account))
        {
            if (_station.GetOwningStation(uid) != station)
                continue;

            UpdateBankAccount(
                (station, account),
                component.Bounty,
                _cargo.CreateAccountDistribution((station, account)));
        }

        Dirty(uid, component);
    }

    private void OnDamageChanged(EntityUid uid, MailComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        if (!_container.TryGetContainer(uid, "contents", out var contents))
            return;

        // Transfer damage to the contents.
        // This should be a general-purpose feature for all containers in the future.
        foreach (var entity in contents.ContainedEntities.ToArray())
        {
            _damageable.TryChangeDamage(entity, args.DamageDelta);
        }
    }

    private void OnDestruction(EntityUid uid, MailComponent component, DestructionEventArgs args)
    {
        if (component.IsLocked)
        {
            // DeltaV - Tampered mail recorded to logistic stats
            ExecuteForEachLogisticsStats(uid,
                (station, logisticStats) =>
                {
                    _logisticsStats.AddTamperedMailLosses(station,
                        logisticStats,
                        component.IsProfitable ? component.Penalty : 0);
                });

            PenalizeStationFailedDelivery(uid, component, "mail-penalty-lock");
        }

        if (!_tag.HasTag(uid, TrashTag))
            OpenMail(uid, component);

        UpdateAntiTamperVisuals(uid, false);
    }

    private void OnBreak(EntityUid uid, MailComponent component, BreakageEventArgs args)
    {
        _appearance.SetData(uid, MailVisuals.IsBroken, true);

        if (!component.IsFragile)
            return;
        // DeltaV - Broken mail recorded to logistic stats
        ExecuteForEachLogisticsStats(uid,
            (station, logisticStats) =>
            {
                _logisticsStats.AddDamagedMailLosses(station,
                    logisticStats,
                    component.IsProfitable ? component.Penalty : 0);
            });

        PenalizeStationFailedDelivery(uid, component, "mail-penalty-fragile");
    }


    private void OnExamined(EntityUid uid, MailComponent component, ref ExaminedEvent args)
    {
        var mailEntityStrings = component.IsLarge ? MailConstants.MailLarge : MailConstants.Mail;

        if (!args.IsInDetailsRange)
        {
            args.PushMarkup(Loc.GetString(mailEntityStrings.DescFar));
            return;
        }

        args.PushMarkup(Loc.GetString(mailEntityStrings.DescClose,
            ("name", component.Recipient),
            ("job", component.RecipientJob)));

        if (component.IsFragile)
            args.PushMarkup(Loc.GetString("mail-desc-fragile"));

        if (component.IsPriority)
            args.PushMarkup(Loc.GetString(component.IsProfitable ? "mail-desc-priority" : "mail-desc-priority-inactive"));
    }

    /// <summary>
    /// Handle the <see cref="GotEmaggedEvent"/> by unlocking the mail without giving cargo money.
    /// </summary>
    private void OnEmagged(EntityUid uid, MailComponent component, ref GotEmaggedEvent args)
    {
        if (!component.IsLocked)
            return;

        UnlockMail(uid, component);

        _popup.PopupPredicted(Loc.GetString("mail-unlocked-by-emag"), uid, args.UserUid);

        _audio.PlayPredicted(component.EmagSound, uid, args.UserUid, AudioParams.Default.WithVolume(4));
        component.IsProfitable = false;
        args.Handled = true;
        Dirty(uid, component);
    }

    /// <summary>
    /// Try to open the mail.
    /// </summary>
    private void OnUseInHand(EntityUid uid, MailComponent component, ref UseInHandEvent args)
    {
        if (!_tag.HasTag(uid, TrashTag))
            return;

        if (component.IsLocked)
        {
            _popup.PopupPredicted(Loc.GetString("mail-locked"), uid, args.User);
            args.Handled = true;
            return;
        }

        args.Handled = true;
        OpenMail(uid, component, args.User);
    }

    private void OpenMail(EntityUid uid, MailComponent? component = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _audio.PlayPredicted(component.OpenSound, uid, user);

        if (user != null)
            _hands.TryDrop((EntityUid)user);

        if (!_container.TryGetContainer(uid, "contents", out var contents))
            return;

        foreach (var entity in contents.ContainedEntities.ToArray())
        {
            _hands.PickupOrDrop(user, entity);
        }

        _tag.AddTag(uid, TrashTag);
        _tag.AddTag(uid, RecyclableTag);
        UpdateMailTrashState(uid, true);
    }

    /// <summary>
    /// Handle logic similar between a normal mail unlock and an emag
    /// frying out the lock.
    /// </summary>
    private void UnlockMail(EntityUid uid, MailComponent component)
    {
        component.IsLocked = false;
        UpdateAntiTamperVisuals(uid, false);

        if (!component.IsPriority)
            return;

        // This is a successful delivery. Keep the failure timer from triggering.
        component.PriorityCancelToken?.Cancel();

        // The priority tape is visually considered to be a part of the
        // anti-tamper lock, so remove that too.
        _appearance.SetData(uid, MailVisuals.IsPriority, false);

        // The examination code depends on this being false to not show
        // the priority tape description anymore.
        component.IsPriority = false;

        Dirty(uid, component);

        RemComp<StealTargetComponent>(uid);
    }

    private void UpdateAntiTamperVisuals(EntityUid uid, bool isLocked)
    {
        _appearance.SetData(uid, MailVisuals.IsLocked, isLocked);
    }

    private void UpdateMailTrashState(EntityUid uid, bool isTrash)
    {
        _appearance.SetData(uid, MailVisuals.IsTrash, isTrash);
    }

    protected virtual void UpdateBankAccount(
        Entity<StationBankAccountComponent?> ent,
        int balanceAdded,
        Dictionary<ProtoId<CargoAccountPrototype>, double> accountDistribution)
    {
    }

    protected virtual void PenalizeStationFailedDelivery(EntityUid uid,
        MailComponent component,
        string localizationString)
    {
    }


    protected void ExecuteForEachLogisticsStats(EntityUid uid,
        Action<EntityUid, StationLogisticStatsComponent> action)
    {

        var query = EntityQueryEnumerator<StationLogisticStatsComponent, TransformComponent>();
        while (query.MoveNext(out var station, out var logisticStats, out var xform))
        {
            if (_station.GetOwningStation(uid, xform) != station)
                continue;
            action(station, logisticStats);
        }
    }
}

/// <summary>
/// Constants related to mail.
/// </summary>
public static class MailConstants
{
    /// <summary>
    /// Locale strings related to small parcels.
    /// </summary>
    public static readonly MailEntityStrings Mail = new()
    {
        NameAddressed = "mail-item-name-addressed",
        DescClose = "mail-desc-close",
        DescFar = "mail-desc-far",
    };

    /// <summary>
    /// Locale strings related to large packages.
    /// </summary>
    public static readonly MailEntityStrings MailLarge = new()
    {
        NameAddressed = "mail-large-item-name-addressed",
        DescClose = "mail-large-desc-close",
        DescFar = "mail-large-desc-far",
    };
}

/// <summary>
/// A set of localized strings related to mail entities
/// </summary>
public struct MailEntityStrings
{
    public string NameAddressed;
    public string DescClose;
    public string DescFar;
}
