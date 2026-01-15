using System.Linq;
using System.Numerics;
using System.Text;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Radio.EntitySystems;
using Content.Server.Singularity.Components;
using Content.Server.Singularity.EntitySystems;
using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared._Impstation.CCVar;
using Content.Shared._Impstation.Supermatter.Prototypes;
using Content.Shared._Impstation.Supermatter.Systems;
using Content.Shared.Atmos;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Spawners;

namespace Content.Server._Impstation.Supermatter.Systems;

public sealed partial class SupermatterSystem : SharedSupermatterSystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly GravityWellSystem _gravityWell = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly RadioSystem _radio = default!;

    /// <summary>
    /// This is used for the gravitational disturbances produced by the supermatter.
    /// </summary>
    private EntityQuery<GravityWellComponent> _gravityWellQuery;
    
    /// <summary>
    /// This is used to update the supermatter's glow.
    /// </summary>
    private EntityQuery<PointLightComponent> _lightQuery;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        
        InitializeAtmosTick();
        InitializeCollision();
        
        _gravityWellQuery = GetEntityQuery<GravityWellComponent>();
        _lightQuery = GetEntityQuery<PointLightComponent>();
        
        SubscribeLocalEvent<SupermatterComponent, SupermatterDoAfterEvent>(OnGetSliver);
        SubscribeLocalEvent<SupermatterComponent, GravPulseEvent>(OnGravPulse);
        
        SubscribeLocalEvent<SupermatterComponent, SupermatterStatusChangedEvent>(OnSupermatterStatusChanged);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDamagedEvent>(OnSupermatterDamaged);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDelaminationStartedEvent>(OnSupermatterDelaminationStarted);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDelaminationCancelledEvent>(OnSupermatterDelaminationCancelled);
        SubscribeLocalEvent<SupermatterComponent, SupermatterAnnouncementEvent>(OnSupermatterAnnouncement);
    }

    protected override void OnSupermatterDelamination(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationEvent args)
    {
        if (sm.PreferredDelamination is null)
        {
            _adminLog.Add(LogType.Unknown, LogImpact.Extreme, $"{EntityManager.ToPrettyString(uid):uid} failed to choose a delamination type and was deleted at {Transform(uid).Coordinates:coordinates}");
            _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} failed to choose a delamination type and was deleted");
            
            // No delamination type was chosen, and no default was specified. Just delete the supermatter.
            PredictedQueueDel(uid);
            return;
        }

        _adminLog.Add(LogType.Unknown, LogImpact.Medium, $"{EntityManager.ToPrettyString(uid):uid} delaminated with type {sm.PreferredDelamination.ID} at {Transform(uid).Coordinates:coordinates}");
        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} delaminated with type {sm.PreferredDelamination.ID} at {Transform(uid).Coordinates:coordinates}");

        var xform = Transform(uid);
        var mapId = xform.MapID;
        var mapFilter = Filter.BroadcastMap(mapId);
        var message = Loc.GetString("supermatter-delam-player");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        
        // Send the reality distortion message to every player on the map
        _chatManager.ChatMessageToManyFiltered(mapFilter,
            ChatChannel.Server,
            message,
            wrappedMessage,
            uid,
            false,
            true,
            Color.Red);

        // Play the reality distortion sound for every player on the map
        Audio.PlayGlobal(sm.DistortSound, mapFilter, true);
        
        foreach (var gameRule in sm.PreferredDelamination.GameRules)
        {
            // delamination game rules
            var gameRuleEnt = _gameTicker.AddGameRule(gameRule);
            _gameTicker.StartGameRule(gameRuleEnt);
        }
        
        // Give effects to every mob on the map, except those in EntityStorage (lockers, etc)
        var mobLookup = new HashSet<Entity<MobStateComponent>>();
        EntityLookup.GetEntitiesOnMap(mapId, mobLookup);
        var insideEntityStorageQuery = GetEntityQuery<InsideEntityStorageComponent>();
        
        foreach (var mob in mobLookup)
        {
            if (insideEntityStorageQuery.HasComp(mob)) continue;
            
            Effects.ApplyEffects(mob, sm.PreferredDelamination.MobEffects);
        }
        
        Effects.ApplyEffects(uid, sm.PreferredDelamination.SupermatterEffects);
        
        // Not every delamination will automatically destroy the supermatter.
        // So we're going to queue it for deletion just to be sure.
        PredictedQueueDel(uid);
    }

    protected override void UpdateSupermatter(Entity<SupermatterComponent> ent, float frameTime)
    {
        if(ent.Comp.AnnounceNext.HasValue && ent.Comp.AnnounceNext.Value <= Timing.CurTime)
        {
            SetNextAnnouncementTime(ent.AsNullable());
                
            var ev = new SupermatterAnnouncementEvent();
            RaiseLocalEvent(ent, ref ev, true);
        }
    }

    /// <summary>
    /// Decide on how to delaminate.
    /// </summary>
    private SupermatterDelaminationPrototype? ChooseDelamType(EntityUid uid, SupermatterComponent sm)
    {
        _proto.Resolve(sm.DefaultDelamination, out var defaultDelam);
        
        if (sm.EnabledDelaminations.Count == 0)
        {
            return defaultDelam;
        }

        foreach (var protoId in sm.EnabledDelaminations)
        {
            if (!_proto.Resolve(protoId, out var delam))
                continue;
            
            if (CheckDelaminationRequirements((uid, sm), delam.Requirements))
                return delam;
        }

        return defaultDelam;
    }

    /// <summary>
    /// Generate temporary anomalies depending on accumulated power.
    /// </summary>
    private void GenerateAnomalies(EntityUid uid, SupermatterComponent sm)
    {
        var xform = Transform(uid);
        var anomalies = new List<string>();

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        // Bluespace anomaly: ~1/150 chance
        if (Random.Prob(1 / sm.AnomalyBluespaceChance))
            anomalies.Add(sm.AnomalyBluespaceSpawnPrototype);

        // Gravity anomaly: ~1/150 chance above SeverePowerPenaltyThreshold, or ~1/750 chance otherwise
        if (sm.Power > Config.GetCVar(ImpCCVars.SupermatterSeverePowerPenaltyThreshold) && Random.Prob(1 / sm.AnomalyGravityChanceSevere) ||
            Random.Prob(1 / sm.AnomalyGravityChance))
            anomalies.Add(sm.AnomalyGravitySpawnPrototype);

        // Pyroclastic anomaly: ~1/375 chance above SeverePowerPenaltyThreshold, or ~1/2500 chance above PowerPenaltyThreshold
        if (sm.Power > Config.GetCVar(ImpCCVars.SupermatterSeverePowerPenaltyThreshold) && Random.Prob(1 / sm.AnomalyPyroChanceSevere) ||
            sm.Power > Config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold) && Random.Prob(1 / sm.AnomalyPyroChance))
            anomalies.Add(sm.AnomalyPyroSpawnPrototype);

        var count = anomalies.Count;
        if (count == 0)
            return;

        var tiles = GetSpawningPoints(uid, sm, count);
        if (tiles == null)
            return;

        foreach (var tileref in tiles)
        {
            var anomaly = Spawn(Random.Pick(anomalies), Map.ToCenterCoordinates(tileref, grid));
            EnsureComp<TimedDespawnComponent>(anomaly).Lifetime = sm.AnomalyLifetime;
        }
    }

    /// <summary>
    /// Gets random points around the supermatter.
    /// Most of this is from GetSpawningPoints() in SharedAnomalySystem
    /// </summary>
    private List<TileRef>? GetSpawningPoints(EntityUid uid, SupermatterComponent sm, int amount)
    {
        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;

        var localpos = xform.Coordinates.Position;
        var tilerefs = Map.GetLocalTilesIntersecting(
            xform.GridUid.Value,
            grid,
            new Box2(localpos + new Vector2(-sm.AnomalySpawnMaxRange, -sm.AnomalySpawnMaxRange), localpos + new Vector2(sm.AnomalySpawnMaxRange, sm.AnomalySpawnMaxRange)))
            .ToList();

        if (tilerefs.Count == 0)
            return null;

        var physQuery = GetEntityQuery<PhysicsComponent>();
        var resultList = new List<TileRef>();
        while (resultList.Count < amount)
        {
            if (tilerefs.Count == 0)
                break;

            var tileref = Random.Pick(tilerefs);
            var distance = MathF.Sqrt(MathF.Pow(tileref.X - xform.LocalPosition.X, 2) + MathF.Pow(tileref.Y - xform.LocalPosition.Y, 2));

            // Cut outer & inner circle
            if (distance > sm.AnomalySpawnMaxRange || distance < sm.AnomalySpawnMinRange)
            {
                tilerefs.Remove(tileref);
                continue;
            }

            var valid = true;

            foreach (var ent in Map.GetAnchoredEntities(xform.GridUid.Value, grid, tileref.GridIndices))
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;

                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int)CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }

            if (!valid)
            {
                tilerefs.Remove(tileref);
                continue;
            }

            resultList.Add(tileref);
        }

        return resultList;
    }

    private void LogFirstPower(EntityUid uid, SupermatterComponent sm, EntityUid target)
    {
        _adminLog.Add(LogType.Unknown, LogImpact.Extreme, $"{EntityManager.ToPrettyString(uid):uid} was powered for the first time by {EntityManager.ToPrettyString(target):target} at {Transform(uid).Coordinates:coordinates}");
        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} was powered for the first time by {EntityManager.ToPrettyString(target):target}");
        sm.HasBeenPowered = true;
    }

    private void LogFirstPower(EntityUid uid, SupermatterComponent sm, GasMixture gas)
    {
        _adminLog.Add(LogType.Unknown, LogImpact.Extreme, $"{EntityManager.ToPrettyString(uid):uid} was powered for the first time by gas mixture at {Transform(uid).Coordinates:coordinates}");
        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} was powered for the first time by gas mixture");
        sm.HasBeenPowered = true;
    }

    private void OnGetSliver(EntityUid uid, SupermatterComponent sm, ref SupermatterDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        // Your criminal actions will not go unnoticed
        sm.Damage += sm.DamageDelaminationThreshold / 10.0f;

        var integrity = GetIntegrity((uid, sm)).ToString("0.00");
        SendSupermatterAnnouncement(uid, sm, Loc.GetString("supermatter-announcement-cc-tamper", ("integrity", integrity)));

        Spawn(sm.SliverPrototype, Transform(args.User).Coordinates);
        _popup.PopupClient(Loc.GetString("supermatter-tamper-end"), uid, args.User);

        sm.DelaminationDelay /= 2;
        DirtyField(uid, sm, nameof(SupermatterComponent.DelaminationDelay));
    }

    private void OnGravPulse(Entity<SupermatterComponent> ent, ref GravPulseEvent args)
    {
        if (!_gravityWellQuery.TryComp(ent, out var gravityWell))
            return;

        var nextPulse = 0.5f * Random.NextFloat(1f, 30f);
        _gravityWell.SetPulsePeriod(ent, TimeSpan.FromSeconds(nextPulse), gravityWell);

        var audioParams = AudioParams.Default.WithMaxDistance(gravityWell.MaxRange);
        Audio.PlayPvs(ent.Comp.PullSound, ent, audioParams);
    }

    private void OnSupermatterAnnouncement(EntityUid uid, SupermatterComponent sm, SupermatterAnnouncementEvent args)
    {
        if (sm.SuppressAnnouncements)
            return;
        
        // We do not need to send any announcements if the supermatter is not damaged.
        if (MathHelper.CloseTo(sm.Damage, 0.0f, 0.05f))
            return;
        
        var integrity = GetIntegrity((uid, sm)).ToString("0.00");
        var isHealing = sm.Damage < sm.DamageArchived;
        var isTakingDamage = sm.Damage > sm.DamageArchived;
        
        switch (sm.Status)
        {
            case SupermatterStatusType.Delaminating when sm.IsDelaminationAnnounced && sm.DelaminationTime.HasValue:
            {
                var seconds = Math.Ceiling(sm.DelaminationTime.Value.TotalSeconds - Timing.CurTime.TotalSeconds);
                
                var message = seconds switch
                {
                    > 60 => Loc.GetString("supermatter-time-before-delam", ("time", sm.DelaminationTime.Value)),
                    < 5 => Loc.GetString("supermatter-seconds-before-delam-imminent", ("seconds", seconds)),
                    _ => Loc.GetString("supermatter-seconds-before-delam-countdown", ("seconds", seconds)),
                };
                
                if (seconds < 5 && SpeechQuery.TryComp(uid, out var speech))
                    speech.SoundCooldownTime = 4.5f;
            
                SendSupermatterAnnouncement(uid, sm, message, true);
                break;
            }
            case >= SupermatterStatusType.Warning when isHealing:
            {
                var message = Loc.GetString("supermatter-healing", ("integrity", integrity));
                var global = sm.Status >= SupermatterStatusType.Emergency;

                if (SpeechQuery.TryComp(uid, out var speech))
                    // Reset speech cooldown after healing is started
                    speech.SoundCooldownTime = 0.5f;
            
                SendSupermatterAnnouncement(uid, sm, message, global);
                break;
            }
            case >= SupermatterStatusType.Warning when isTakingDamage && !sm.IsDelaminating:
            {
                // We don't want to send the 0% integrity message, and we only want to emit the warning if the supermatter is taking damage. 
                var isEmergency = sm.Damage >= sm.DamageEmergencyThreshold;
                var message = Loc.GetString( isEmergency? "supermatter-emergency" : "supermatter-warning", ("integrity", integrity));
                SendSupermatterAnnouncement(uid, sm, message, isEmergency);

                if (sm.Power >= Config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold))
                {
                    SendSupermatterAnnouncement(uid, sm, Loc.GetString(sm.PowerlossInhibitor >= 0.5 ? "supermatter-threshold-power" : "supermatter-threshold-powerloss"));
                }
                
                if (sm.GasStorage != null && sm.GasStorage.TotalMoles >= Config.GetCVar(ImpCCVars.SupermatterMolePenaltyThreshold))
                {
                    message = Loc.GetString("supermatter-threshold-mole");
                    SendSupermatterAnnouncement(uid, sm, message);
                }
                
                break;
            }
        }
    }

    private void OnSupermatterDamaged(EntityUid uid, SupermatterComponent sm, SupermatterDamagedEvent args)
    {   
        if (sm.Damage >= sm.DamageDelaminationThreshold && !sm.IsDelaminating)
        {
            // Start the delamination process
            sm.IsDelaminating = true;
            sm.DelaminationTime = Timing.CurTime + sm.DelaminationDelay;
            DirtyFields(uid, sm, MetaData(uid), nameof(SupermatterComponent.IsDelaminating), nameof(SupermatterComponent.DelaminationTime));

            var ev = new SupermatterDelaminationStartedEvent();
            RaiseLocalEvent(uid, ref ev);
        }
        else if (sm.Damage < sm.DamageDelaminationThreshold && sm.IsDelaminating)
        {
            // Cancel the delamination process
            sm.IsDelaminating = false;
            sm.DelaminationTime = null;
            DirtyFields(uid, sm, MetaData(uid), nameof(SupermatterComponent.IsDelaminating), nameof(SupermatterComponent.DelaminationTime));

            var ev = new SupermatterDelaminationCancelledEvent();
            RaiseLocalEvent(uid, ref ev);
        }
    }

    private void OnSupermatterDelaminationCancelled(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationCancelledEvent args)
    {
        _adminLog.Add(LogType.Unknown, LogImpact.Medium, $"{EntityManager.ToPrettyString(uid):uid} delamination cancelled");

        sm.IsDelaminationAnnounced = false;
        sm.PreferredDelamination = null;
        DirtyField(uid, sm, nameof(SupermatterComponent.PreferredDelamination));
        
        var integrity = GetIntegrity((uid, sm)).ToString("0.00");
        SendSupermatterAnnouncement(uid, sm, Loc.GetString("supermatter-delam-cancel", ("integrity", integrity)), true);
    }

    private void OnSupermatterDelaminationStarted(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationStartedEvent args)
    {
        var sb = new StringBuilder();
        sm.PreferredDelamination ??= ChooseDelamType(uid, sm);
        DirtyField(uid, sm, nameof(SupermatterComponent.PreferredDelamination));
        
        _adminLog.Add(LogType.Unknown, LogImpact.Medium, $"{EntityManager.ToPrettyString(uid):uid} delamination started with type {sm.PreferredDelamination?.ID ?? "None"}");
        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} delamination started with type {sm.PreferredDelamination?.ID ?? "None"}");

        sb.AppendLine(Loc.GetString(sm.PreferredDelamination?.Message ?? "supermatter-delam-generic"));
        sb.Append(Loc.GetString("supermatter-time-before-delam", ("time", sm.DelaminationDelay)));

        sm.IsDelaminationAnnounced = true;
        SendSupermatterAnnouncement(uid, sm, sb.ToString(), true);
        
        SetNextAnnouncementTime((uid, sm));
    }

    private void OnSupermatterStatusChanged(Entity<SupermatterComponent> ent, ref SupermatterStatusChangedEvent args)
    {
        UpdateAppearanceFromState(ent);
        UpdateLinkedPorts(ent);
        UpdateSpeech(ent);
        UpdateAmbient(ent);

        // We should give the supermatter a chance to announce a few seconds after the status changes.
        // Only do this for less than delaminating status so we don't clobber the ominous countdown.
        if(ent.Comp.Status < SupermatterStatusType.Delaminating)
            SetNextAnnouncementTime(ent.AsNullable(), TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Sends the given message to local chat and a radio channel
    /// </summary>
    /// <param name="global">If true, sends the message to the common radio</param>
    private void SendSupermatterAnnouncement(EntityUid uid, SupermatterComponent sm, string message, bool global = false)
    {
        if (sm.SuppressAnnouncements)
            return;

        if (string.IsNullOrEmpty(message))
            return;

        var channel = global ? sm.ChannelGlobal : sm.Channel;

        _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
        _radio.SendRadioMessage(uid, message, channel, uid);
    }

    /// <summary>
    /// Checks the supermatter's status and updates it accordingly, then raises a status changed event if it has changed.
    /// </summary>
    private void UpdateSupermatterStatus(Entity<SupermatterComponent> ent)
    {
        var currentStatus = CalculateSupermatterStatus(ent.AsNullable());

        if (ent.Comp.Status == currentStatus)
            return;
        
        ent.Comp.Status = currentStatus;
        var ev = new SupermatterStatusChangedEvent();
        RaiseLocalEvent(ent, ref ev);
    }
}
