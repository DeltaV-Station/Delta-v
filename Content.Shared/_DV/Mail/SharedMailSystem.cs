using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._DV.Cargo.Components;
using Content.Shared._DV.Cargo.Systems;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Objectives.Components;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Content.Shared.Station;
using Content.Shared.Tag;
using Content.Shared.Tools.Systems;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Mail;

public abstract class SharedMailSystem : EntitySystem
{
    [Dependency] protected readonly AccessReaderSystem Access = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly LogisticStatsSystem LogisticsStats = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly SharedCargoSystem _cargo = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] protected readonly SharedStationSystem Station = default!;
    [Dependency] protected readonly TagSystem Tag = default!;

    private static readonly ProtoId<TagPrototype> RecyclableTag = "Recyclable";
    private static readonly ProtoId<TagPrototype> TrashTag = "Trash";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MailComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<MailComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<MailComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MailComponent, DamageDealtEvent>(OnDamageDealt);
        SubscribeLocalEvent<MailComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<MailComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MailComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<MailComponent, UseInHandEvent>(OnUseInHand, before: new[] { typeof(IngestionSystem) });
        SubscribeLocalEvent<MailComponent, MailToolUseDoAfterEvent>(OnOpenWithTool);
    }

    /// <summary>
    /// Handle the <see cref="ComponentShutdown"/> and cancel any pending CancellationTokenSources for priority mail.
    /// </summary>
    private static void OnShutdown(Entity<MailComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.PriorityCancelToken?.Cancel();
    }

    /// <summary>
    /// Checks if the mail should be opened using a tool
    /// </summary>
    private void OnInteractUsing(Entity<MailComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !ent.Comp.IsLocked)
            return;

        var toolDoAfter = new MailToolUseDoAfterEvent();
        args.Handled = _tool.UseTool(args.Used,
            args.User,
            ent.Owner,
            ent.Comp.OpenToolTime,
            ent.Comp.OpenToolQualities,
            toolDoAfter,
            out _);
    }

    /// <summary>
    /// Handle the <see cref="AfterInteractEvent"/> by checking the ID against the mail.
    /// </summary>
    private void OnAfterInteractUsing(Entity<MailComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (!args.CanReach || !ent.Comp.IsLocked)
            return;

        if (!HasComp<AccessReaderComponent>(ent))
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

        if (!HasComp<EmaggedComponent>(ent))
        {
            if (idCard.FullName != ent.Comp.Recipient || idCard.LocalizedJobTitle != ent.Comp.RecipientJob)
            {
                _popup.PopupPredicted(Loc.GetString("mail-recipient-mismatch"), ent, args.User);
                return;
            }

            if (!Access.IsAllowed(ent, args.User))
            {
                _popup.PopupPredicted(Loc.GetString("mail-invalid-access"), ent, args.User);
                return;
            }
        }

        // DeltaV - Add earnings to logistic stats
        ExecuteForEachLogisticsStats(ent,
            (station, logisticStats) =>
            {
                LogisticsStats.AddOpenedMailEarnings(station,
                    logisticStats,
                    ent.Comp.IsProfitable ? ent.Comp.Bounty : 0);
            });

        UnlockMail(ent);

        if (!ent.Comp.IsProfitable)
        {
            _popup.PopupPredicted(Loc.GetString("mail-unlocked"), ent, args.User);
            return;
        }

        _popup.PopupPredicted(Loc.GetString("mail-unlocked-reward", ("bounty", ent.Comp.Bounty)), ent, args.User);
        ent.Comp.IsProfitable = false;

        var query = EntityQueryEnumerator<StationBankAccountComponent>();
        while (query.MoveNext(out var station, out var account))
        {
            if (Station.GetOwningStation(ent) != station)
                continue;

            UpdateBankAccount(
                (station, account),
                ent.Comp.Bounty,
                _cargo.CreateAccountDistribution((station, account)));
        }

        Dirty(ent);
    }

    /// <summary>
    /// Handle the <see cref="DamageDealtEvent"/> and transfer damage to the contents.
    /// </summary>
    private void OnDamageDealt(Entity<MailComponent> ent, ref DamageDealtEvent args)
    {
        if (!_container.TryGetContainer(ent, "contents", out var contents))
            return;

        // Transfer damage to the contents.
        // This should be a general-purpose feature for all containers in the future.
        foreach (var entity in contents.ContainedEntities.ToArray())
        {
            _damageable.TryChangeDamage(entity, args.Damage);
        }
    }

    /// <summary>
    /// Handle the <see cref="DestructionEventArgs"/>.
    /// </summary>
    private void OnDestruction(Entity<MailComponent> ent, ref DestructionEventArgs args)
    {
        if (ent.Comp.IsLocked)
            PenalizeTamperedMail(ent);

        if (!Tag.HasTag(ent, TrashTag))
            OpenMail(ent.AsNullable());

        UpdateAntiTamperVisuals(ent, false);
    }

    /// <summary>
    /// Handle the <see cref="BreakageEventArgs"/>.
    /// </summary>
    private void OnBreak(Entity<MailComponent> ent, ref BreakageEventArgs args)
    {
        Appearance.SetData(ent, MailVisuals.IsBroken, true);

        if (!ent.Comp.IsFragile)
            return;

        ExecuteForEachLogisticsStats(ent,
            (station, logisticStats) =>
            {
                LogisticsStats.AddDamagedMailLosses(station,
                    logisticStats,
                    ent.Comp.IsProfitable ? ent.Comp.Penalty : 0);
            });

        PenalizeStationFailedDelivery(ent, "mail-penalty-fragile");
    }

    /// <summary>
    /// Handle the <see cref="ExaminedEvent"/>.
    /// </summary>
    private void OnExamined(Entity<MailComponent> ent, ref ExaminedEvent args)
    {
        var mailEntityStrings = ent.Comp.IsLarge ? MailConstants.MailLarge : MailConstants.Mail;

        if (!args.IsInDetailsRange)
        {
            args.PushMarkup(Loc.GetString(mailEntityStrings.DescFar));
            return;
        }

        args.PushMarkup(Loc.GetString(mailEntityStrings.DescClose,
            ("name", ent.Comp.Recipient),
            ("job", ent.Comp.RecipientJob)));

        if (ent.Comp.IsFragile)
            args.PushMarkup(Loc.GetString("mail-desc-fragile"));

        if (ent.Comp.IsPriority)
        {
            if (ent.Comp.ExpiryTime != null && ent.Comp.IsProfitable)
            {
                var timeLeft = ent.Comp.ExpiryTime.Value - Timing.CurTime;
                if (timeLeft > TimeSpan.Zero)
                {
                    args.PushMarkup(Loc.GetString("mail-desc-priority-timer",
                        ("time", timeLeft.ToString(@"mm\:ss"))));
                }
                else
                {
                    args.PushMarkup(Loc.GetString("mail-desc-priority-inactive"));
                }
            }
            else
            {
                // Handle the weird case of the timer not being set but mail being priority, if that ever happens
                args.PushMarkup(Loc.GetString(ent.Comp.IsProfitable ? "mail-desc-priority" : "mail-desc-priority-inactive"));
            }
        }
    }

    /// <summary>
    /// Handle the <see cref="GotEmaggedEvent"/> by unlocking the mail without giving cargo money.
    /// </summary>
    private void OnEmagged(Entity<MailComponent> ent, ref GotEmaggedEvent args)
    {
        if (!ent.Comp.IsLocked)
            return;

        UnlockMail(ent, args.UserUid, true);
        PenalizeTamperedMail(ent);

        Audio.PlayPredicted(ent.Comp.EmagSound, ent, args.UserUid, AudioParams.Default.WithVolume(4));
        ent.Comp.IsProfitable = false;
        args.Handled = true;
        Dirty(ent);
    }

    /// <summary>
    /// Handle the <see cref="UseInHandEvent"/> and try to open the mail.
    /// </summary>
    private void OnUseInHand(Entity<MailComponent> ent, ref UseInHandEvent args)
    {
        if (Tag.HasTag(ent, TrashTag))
            return;

        if (ent.Comp.IsLocked)
        {
            _popup.PopupPredicted(Loc.GetString("mail-locked"), ent, args.User);
            args.Handled = true;
            return;
        }

        args.Handled = true;
        OpenMail(ent.AsNullable(), args.User);
    }

    private void OnOpenWithTool(Entity<MailComponent> ent, ref MailToolUseDoAfterEvent args)
    {
        if (!ent.Comp.IsLocked)
            return;

        UnlockMail(ent, args.User, true);
        PenalizeTamperedMail(ent);
        ent.Comp.IsProfitable = false;

        Dirty(ent);
    }

    /// <summary>
    /// Helper method for actually opening the mail.
    /// </summary>
    private void OpenMail(Entity<MailComponent?> ent, EntityUid? user = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        Audio.PlayPredicted(ent.Comp.OpenSound, ent, user);

        if (user != null)
            _hands.TryDrop((EntityUid)user);

        if (!_container.TryGetContainer(ent, "contents", out var contents))
            return;

        foreach (var entity in contents.ContainedEntities.ToArray())
        {
            _hands.PickupOrDrop(user, entity);
        }

        Tag.AddTag(ent, TrashTag);
        Tag.AddTag(ent, RecyclableTag);
        UpdateMailTrashState(ent, true);
    }

    /// <summary>
    /// Handle logic similar between a normal mail unlock and an emag
    /// frying out the lock.
    /// </summary>
    private void UnlockMail(Entity<MailComponent> ent, EntityUid? opener = null, bool fraudulent = false)
    {
        ent.Comp.IsLocked = false;
        UpdateAntiTamperVisuals(ent, false);

        if (ent.Comp.IsPriority)
        {
            // This is a successful delivery. Keep the failure timer from triggering.
            ent.Comp.PriorityCancelToken?.Cancel();

            // The priority tape is visually considered to be a part of the
            // anti-tamper lock, so remove that too.
            Appearance.SetData(ent, MailVisuals.IsPriority, false);

            // The examination code depends on this being false to not show
            // // the priority tape description anymore.
            ent.Comp.IsPriority = false;
        }

        var ev = new MailOpenedEvent(opener, fraudulent);
        RaiseLocalEvent(ent, ref ev);

        Dirty(ent);

        RemComp<StealTargetComponent>(ent);
    }

    private void PenalizeTamperedMail(Entity<MailComponent> ent)
    {
        ExecuteForEachLogisticsStats(ent,
            (station, logisticStats) =>
            {
                LogisticsStats.AddTamperedMailLosses(station,
                    logisticStats,
                    ent.Comp.IsProfitable ? ent.Comp.Penalty : 0);
            });

        PenalizeStationFailedDelivery(ent, "mail-penalty-lock");
    }

    private void UpdateAntiTamperVisuals(EntityUid uid, bool isLocked)
    {
        Appearance.SetData(uid, MailVisuals.IsLocked, isLocked);
    }

    private void UpdateMailTrashState(EntityUid uid, bool isTrash)
    {
        Appearance.SetData(uid, MailVisuals.IsTrash, isTrash);
    }

    /// <summary>
    /// Implemented on the Server, this is a fancy wrapper for the Cargo API since it's not in Shared yet.
    /// </summary>
    protected virtual void UpdateBankAccount(
        Entity<StationBankAccountComponent?> ent,
        int balanceAdded,
        Dictionary<ProtoId<CargoAccountPrototype>, double> accountDistribution)
    {
    }

    /// <summary>
    /// Implemented on the Server side.
    /// </summary>
    protected virtual void PenalizeStationFailedDelivery(Entity<MailComponent> ent, string localizationString)
    {
    }


    protected void ExecuteForEachLogisticsStats(EntityUid uid,
        Action<EntityUid, StationLogisticStatsComponent> action)
    {
        var query = EntityQueryEnumerator<StationLogisticStatsComponent>();
        while (query.MoveNext(out var station, out var logisticStats))
        {
            if (Station.GetOwningStation(uid) != station)
                continue;
            action(station, logisticStats);
        }
    }

    /// <summary>
    /// Try to match a mail receiver to a mail teleporter.
    /// </summary>
    [PublicAPI]
    public bool TryGetMailTeleporterForReceiver(EntityUid receiverUid, [NotNullWhen(true)] out MailTeleporterComponent? teleporterComponent, [NotNullWhen(true)] out EntityUid? teleporterUid)
    {
        var query = EntityQueryEnumerator<MailTeleporterComponent>();
        var receiverStation = Station.GetOwningStation(receiverUid);

        while (query.MoveNext(out var uid, out var mailTeleporter))
        {
            var teleporterStation = Station.GetOwningStation(uid);
            if (receiverStation != teleporterStation)
                continue;
            teleporterComponent = mailTeleporter;
            teleporterUid = uid;
            return true;
        }

        teleporterComponent = null;
        teleporterUid = null;
        return false;
    }

    /// <summary>
    /// Try to construct a recipient struct for a mail parcel based on a receiver.
    /// </summary>
    [PublicAPI]
    public bool TryGetMailRecipientForReceiver(EntityUid receiverUid, [NotNullWhen(true)] out MailRecipient? recipient)
    {
        if (_idCard.TryFindIdCard(receiverUid, out var idCard)
            && TryComp<AccessComponent>(idCard.Owner, out var access)
            && idCard.Comp.FullName != null)
        {
            var accessTags = access.Tags;
            var mayReceivePriorityMail = !(_mind.GetMind(receiverUid) == null);

            recipient = new MailRecipient(
                idCard.Comp.FullName,
                idCard.Comp.LocalizedJobTitle ?? idCard.Comp.JobTitle ?? "Unknown",
                idCard.Comp.JobIcon,
                accessTags,
                mayReceivePriorityMail);

            return true;
        }

        recipient = null;
        return false;
    }

    /// <summary>
    /// Sets whether the mail is fragile.
    /// </summary>
    [PublicAPI]
    public void SetFragile(Entity<MailComponent?> ent, bool isFragile)
    {
        if (!Resolve(ent, ref ent.Comp) || ent.Comp.IsFragile == isFragile)
            return;

        ent.Comp.IsFragile = isFragile;
        Dirty(ent);
    }

    /// <summary>
    /// Sets whether the mail is priority mail.
    /// </summary>
    [PublicAPI]
    public void SetPriority(Entity<MailComponent?> ent, bool isPriority)
    {
        if (!Resolve(ent, ref ent.Comp) || ent.Comp.IsPriority == isPriority)
            return;

        ent.Comp.IsPriority = isPriority;
        Dirty(ent);
    }

    /// <summary>
    /// Sets whether the mail is a large package.
    /// </summary>
    [PublicAPI]
    public void SetLarge(Entity<MailComponent?> ent, bool isLarge)
    {
        if (!Resolve(ent, ref ent.Comp) || ent.Comp.IsLarge == isLarge)
            return;

        ent.Comp.IsLarge = isLarge;
        Dirty(ent);
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

public struct MailRecipient(
    string name,
    string job,
    string jobIcon,
    HashSet<ProtoId<AccessLevelPrototype>> accessTags,
    bool mayReceivePriorityMail)
{
    public readonly string Name = name;
    public readonly string Job = job;
    public readonly string JobIcon = jobIcon;
    public readonly HashSet<ProtoId<AccessLevelPrototype>> AccessTags = accessTags;
    public readonly bool MayReceivePriorityMail = mayReceivePriorityMail;
}

[Serializable, NetSerializable]
public sealed partial class MailToolUseDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone()
    {
        return this;
    }
}
