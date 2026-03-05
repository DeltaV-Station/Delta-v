using Content.Shared.Chat;
using Content.Server.Cargo.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Damage.Components;
using Content.Server._DV.Mail.Components;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared.Destructible.Thresholds.Triggers;
using Content.Server.Destructible;
using Content.Server.Power.Components;
using Content.Server.Radio.EntitySystems; // ImpStation - for radio notifications of new mail
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Cargo.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage.Components;
using Content.Shared._DV.Mail;
using Content.Shared.Fluids.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Radio; // ImpStation - for radio notifications of new mail
using Content.Shared.Roles;
using Content.Shared.Storage;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Power.EntitySystems;
using Timer = Robust.Shared.Timing.Timer;
using Content.Shared.Destructible;
using JetBrains.Annotations;

namespace Content.Server._DV.Mail.EntitySystems;

public sealed class MailSystem : SharedMailSystem
{
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly OpenableSystem _openable = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    private EntityQuery<ApcPowerReceiverComponent> _powerQuery;

    private static readonly ProtoId<TagPrototype> MailTag = "Mail";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning, after: new[] { typeof(SpawnPointSystem) });

        _powerQuery = GetEntityQuery<ApcPowerReceiverComponent>();
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = Timing.CurTime;
        var query = EntityQueryEnumerator<MailTeleporterComponent>();
        while (query.MoveNext(out var uid, out var mailTeleporter))
        {
            if (_powerQuery.TryComp(uid, out var power) && !_powerReceiver.IsPowered((uid, power)))
                continue;

            if (mailTeleporter.NextDelivery > curTime)
                continue;

            SpawnMail((uid, mailTeleporter));

            mailTeleporter.NextDelivery = curTime + mailTeleporter.TeleportInterval;
        }
    }

    /// <inheritdoc/>
    protected override void UpdateBankAccount(
        Entity<StationBankAccountComponent?> ent,
        int balanceAdded,
        Dictionary<ProtoId<CargoAccountPrototype>, double> accountDistribution)
    {
        _cargo.UpdateBankAccount(ent, balanceAdded, accountDistribution);
    }

    /// <summary>
    /// Handle the <see cref="PlayerSpawningEvent"/> by giving them the <see cref="MailReceiverComponent"/>.
    /// </summary>
    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult == null ||
            args.Job == null ||
            args.Station is not { } station)
        {
            return;
        }

        if (!HasComp<StationMailRouterComponent>(station))
            return;

        EnsureComp<MailReceiverComponent>(args.SpawnResult.Value);
    }

    /// <summary>
    /// Penalize a station for a failed delivery.
    /// </summary>
    /// <remarks>
    /// This will mark a parcel as no longer being profitable, which will
    /// prevent multiple failures on different conditions for the same
    /// delivery.
    ///
    /// The standard penalization is breaking the anti-tamper lock,
    /// but this allows a delivery to fail for other reasons too
    /// while having a generic function to handle different messages.
    /// </remarks>
    protected override void PenalizeStationFailedDelivery(Entity<MailComponent> ent, string localizationString)
    {
        if (!ent.Comp.IsProfitable)
            return;

        _chat.TrySendInGameICMessage(ent, Loc.GetString(localizationString, ("credits", ent.Comp.Penalty)), InGameICChatType.Speak, false);
        Audio.PlayPvs(ent.Comp.PenaltySound, ent);

        ent.Comp.IsProfitable = false;

        if (ent.Comp.IsPriority)
            Appearance.SetData(ent, MailVisuals.IsPriorityInactive, true);

        var query = EntityQueryEnumerator<StationBankAccountComponent>();
        while (query.MoveNext(out var station, out var account))
        {
            if (Station.GetOwningStation(ent) != station)
                continue;

            _cargo.UpdateBankAccount(
                (station, account),
                ent.Comp.Penalty,
                _cargo.CreateAccountDistribution((station, account)));
            return;
        }

        Dirty(ent);
    }


    /// <summary>
    /// Returns true if the given entity is considered fragile for delivery.
    /// </summary>
    private bool IsEntityFragile(EntityUid uid, int fragileDamageThreshold)
    {
        // It takes damage on falling.
        if (HasComp<DamageOnLandComponent>(uid))
            return true;

        // It can be spilled easily and has something to spill.
        if (HasComp<SpillableComponent>(uid)
            && TryComp<OpenableComponent>(uid, out var openable)
            && !_openable.IsClosed(uid, null, openable)
            && _solution.PercentFull(uid) > 0)
            return true;

        // It might be made of non-reinforced glass.
        if (TryComp<DamageableComponent>(uid, out var damageableComponent)
            && damageableComponent.DamageModifierSetId == "Glass")
            return true;

        // Fallback: It breaks or is destroyed in less than a damage
        // threshold dictated by the teleporter.
        if (!TryComp<DestructibleComponent>(uid, out var destructibleComp))
            return false;

        foreach (var threshold in destructibleComp.Thresholds)
        {
            if (threshold.Trigger is not DamageTrigger trigger || trigger.Damage >= fragileDamageThreshold)
                continue;

            foreach (var behavior in threshold.Behaviors)
            {
                if (behavior is not DoActsBehavior doActs)
                    continue;

                if (doActs.Acts.HasFlag(ThresholdActs.Breakage) || doActs.Acts.HasFlag(ThresholdActs.Destruction))
                    return true;
            }
        }

        return false;
    }

    private bool TryMatchJobTitleToDepartment(string jobTitle, [NotNullWhen(true)] out string? jobDepartment)
    {
        jobDepartment = null;

        var departments = _prototype.EnumeratePrototypes<DepartmentPrototype>();

        foreach (var department in departments)
        {
            var foundJob = department.Roles
                .Any(role =>
                    _prototype.TryIndex(role, out var jobPrototype)
                    && jobPrototype.LocalizedName == jobTitle);

            if (!foundJob)
                continue;

            jobDepartment = department.ID;
            return true;
        }

        return false;
    }

    private bool TryMatchJobTitleToPrototype(string jobTitle, [NotNullWhen(true)] out JobPrototype? jobPrototype)
    {
        jobPrototype = _prototype
            .EnumeratePrototypes<JobPrototype>()
            .FirstOrDefault(job => job.LocalizedName == jobTitle);

        return jobPrototype != null;
    }

    /// <summary>
    /// Handle all the gritty details particular to a new mail entity.
    /// </summary>
    /// <remarks>
    /// This is separate mostly so the unit tests can get to it.
    /// </remarks>
    /// TODO: Move to shared when IsEntityFragile can be made network-safe
    [PublicAPI]
    public void SetupMail(EntityUid uid, MailTeleporterComponent component, MailRecipient recipient)
    {
        var mailComp = EnsureComp<MailComponent>(uid);

        var container = _container.EnsureContainer<Container>(uid, "contents");
        foreach (var entity in EntitySpawnCollection.GetSpawns(mailComp.Contents, _random).Select(item => EntityManager.SpawnEntity(item, Transform(uid).Coordinates)))
        {
            if (!_container.Insert(entity, container))
            {
                Log.Error($"Can't insert {ToPrettyString(entity)} into new mail delivery {ToPrettyString(uid)}! Deleting it.");
                QueueDel(entity);
            }
            else if (!mailComp.IsFragile && IsEntityFragile(entity, component.FragileDamageThreshold))
            {
                mailComp.IsFragile = true;
            }
        }

        if (_random.Prob(component.PriorityChance))
            mailComp.IsPriority = true;

        // This needs to override both the random probability and the
        // entity prototype, so this is fine.
        if (!recipient.MayReceivePriorityMail)
            mailComp.IsPriority = false;

        mailComp.RecipientJob = recipient.Job;
        mailComp.Recipient = recipient.Name;

        var mailEntityStrings = mailComp.IsLarge ? MailConstants.MailLarge : MailConstants.Mail;
        if (mailComp.IsLarge)
        {
            mailComp.Bounty += component.LargeBonus;
            mailComp.Penalty += component.LargeMalus;
        }

        if (mailComp.IsFragile)
        {
            mailComp.Bounty += component.FragileBonus;
            mailComp.Penalty += component.FragileMalus;
            Appearance.SetData(uid, MailVisuals.IsFragile, true);
        }

        if (mailComp.IsPriority)
        {
            mailComp.Bounty += component.PriorityBonus;
            mailComp.Penalty += component.PriorityMalus;
            Appearance.SetData(uid, MailVisuals.IsPriority, true);

            mailComp.ExpiryTime = Timing.CurTime + component.PriorityDuration;

            mailComp.PriorityCancelToken = new CancellationTokenSource();

            Timer.Spawn((int)component.PriorityDuration.TotalMilliseconds,
                () =>
                {
                    ExecuteForEachLogisticsStats(uid,
                        (station, logisticStats) =>
                        {
                            LogisticsStats.AddExpiredMailLosses(station,
                                logisticStats,
                                mailComp.IsProfitable ? mailComp.Penalty : 0);
                        });

                    PenalizeStationFailedDelivery((uid, mailComp), "mail-penalty-expired");
                },
                mailComp.PriorityCancelToken.Token);
        }

        Appearance.SetData(uid, MailVisuals.JobIcon, recipient.JobIcon);

        _meta.SetEntityName(uid,
            Loc.GetString(mailEntityStrings.NameAddressed,
                ("recipient", recipient.Name)));

        var accessReader = EnsureComp<AccessReaderComponent>(uid);
        foreach (var access in recipient.AccessTags)
        {
            Access.TryAddAccess((uid, accessReader), access);
        }

        Dirty(uid, mailComp);
    }

    /// <summary>
    /// Return the parcels waiting for delivery.
    /// </summary>
    /// <param name="uid">The mail teleporter to check.</param>
    private List<EntityUid> GetUndeliveredParcels(EntityUid uid)
    {
        // An alternative solution would be to keep a list of the unopened
        // parcels spawned by the teleporter and see if they're not carried
        // by someone, but this is simple, and simple is good.
        var coordinates = Transform(uid).Coordinates;
        const LookupFlags lookupFlags = LookupFlags.Dynamic | LookupFlags.Sundries;

        var entitiesInTile = _lookup.GetEntitiesIntersecting(coordinates, lookupFlags);

        return entitiesInTile.Where(HasComp<MailComponent>).ToList();
    }

    /// <summary>
    /// Return how many parcels are waiting for delivery.
    /// </summary>
    /// <param name="uid">The mail teleporter to check.</param>
    private uint GetUndeliveredParcelCount(EntityUid uid)
    {
        return (uint)GetUndeliveredParcels(uid).Count;
    }

    /// <summary>
    /// Get the list of valid mail recipients for a mail teleporter.
    /// </summary>
    private List<MailRecipient> GetMailRecipientCandidates(EntityUid uid)
    {
        var candidateList = new List<MailRecipient>();
        var query = EntityQueryEnumerator<MailReceiverComponent>();
        var teleporterStation = Station.GetOwningStation(uid);

        while (query.MoveNext(out var receiverUid, out _))
        {
            var receiverStation = Station.GetOwningStation(receiverUid);
            if (receiverStation != teleporterStation)
                continue;

            if (TryGetMailRecipientForReceiver(receiverUid, out var recipient))
                candidateList.Add(recipient.Value);
        }

        return candidateList;
    }

    /// <summary>
    /// Handle the spawning of all the mail for a mail teleporter.
    /// </summary>
    private void SpawnMail(Entity<MailTeleporterComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
        {
            Log.Error($"Tried to SpawnMail on {ToPrettyString(ent)} without a valid MailTeleporterComponent!");
            return;
        }

        if (GetUndeliveredParcelCount(ent) >= ent.Comp.MaximumUndeliveredParcels)
            return;

        var candidateList = GetMailRecipientCandidates(ent);

        if (candidateList.Count <= 0)
        {
            Log.Warning("List of mail candidates was empty!");
            return;
        }

        if (!_prototype.TryIndex(ent.Comp.MailPool, out var pool))
        {
            Log.Error($"Can't index {ToPrettyString(ent)}'s MailPool {ent.Comp.MailPool}!");
            return;
        }

        var deliveryCount = ent.Comp.MinimumDeliveriesPerTeleport + candidateList.Count / ent.Comp.CandidatesPerDelivery;

        for (var i = 0; i < deliveryCount; i++)
        {
            var candidate = _random.Pick(candidateList);
            var possibleParcels = new Dictionary<EntProtoId, float>(pool.Everyone);

            if (TryMatchJobTitleToPrototype(candidate.Job, out var jobPrototype)
                && pool.Jobs.TryGetValue(jobPrototype.ID, out var jobParcels))
            {
                foreach (var (key, value) in jobParcels)
                {
                    possibleParcels[key] = value;
                }
            }

            if (TryMatchJobTitleToDepartment(candidate.Job, out var department)
                && pool.Departments.TryGetValue(department, out var departmentParcels))
            {
                foreach (var (key, value) in departmentParcels)
                {
                    possibleParcels[key] = value;
                }
            }

            var accumulated = 0f;
            var randomPoint = _random.NextFloat(possibleParcels.Values.Sum());
            EntProtoId? chosenParcel = null;

            foreach (var parcel in possibleParcels)
            {
                accumulated += parcel.Value;
                if (!(accumulated >= randomPoint))
                    continue;
                chosenParcel = parcel.Key;
                break;
            }

            if (chosenParcel == null)
            {
                Log.Error($"MailSystem wasn't able to find a deliverable parcel for {candidate.Name}, {candidate.Job}!");
                return;
            }

            var coordinates = Transform(ent).Coordinates;
            var mail = EntityManager.SpawnEntity(chosenParcel, coordinates);
            SetupMail(mail, ent.Comp, candidate);

            Tag.AddTag(mail, MailTag);
        }

        if (_container.TryGetContainer(ent, "queued", out var queued))
            _container.EmptyContainer(queued);

        Audio.PlayPvs(ent.Comp.TeleportSound, ent);
        if (ent.Comp.RadioNotification) // ImpStation - for radio notifications of new mail
            Report(ent, ent.Comp.RadioChannel, ent.Comp.ShipmentReceivedMessage, ("timeLeft", ent.Comp.TeleportInterval));
    }

    /// <summary>
    /// ImpStation
    /// Send a radio notification about new mail
    /// </summary>
    private void Report(EntityUid source, ProtoId<RadioChannelPrototype> channel, string messageKey, params (string, object)[] args)
    {
        var message = args.Length == 0 ? Loc.GetString(messageKey) : Loc.GetString(messageKey, args);
        _radio.SendRadioMessage(source, message, channel, source);
    }

    /// <summary>
    /// Sets the next delivery time for the mail teleporter.
    /// </summary>
    [PublicAPI]
    public void SetNextDeliveryTime(Entity<MailTeleporterComponent?> ent, TimeSpan nextDeliveryTime)
    {
        if (!Resolve(ent, ref ent.Comp) || ent.Comp.NextDelivery == nextDeliveryTime)
            return;

        ent.Comp.NextDelivery = nextDeliveryTime;
    }

    /// <summary>
    /// Triggers an immediate mail delivery for the teleporter, bypassing the normal timer.
    /// </summary>
    [PublicAPI]
    public void DeliverNow(Entity<MailTeleporterComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        SpawnMail(ent);
        ent.Comp.NextDelivery = Timing.CurTime + ent.Comp.TeleportInterval;
    }
}
