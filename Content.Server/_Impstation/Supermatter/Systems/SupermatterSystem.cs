using System.Text;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Examine;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Server.Singularity.Components;
using Content.Server.Singularity.EntitySystems;
using Content.Server.Traits.Assorted;
using Content.Shared._DV.Vision.Components;
using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared._Impstation.CCVar;
using Content.Shared._Impstation.Supermatter.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.Audio;
using Content.Shared.Chat;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.DeviceLinking;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Light.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Radiation.Components;
using Content.Shared.Speech;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Impstation.Supermatter.Systems;

public sealed partial class SupermatterSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly GravityWellSystem _gravityWell = default!;
    [Dependency] private readonly ParacusiaSystem _paracusia = default!;
    [Dependency] private readonly PointLightSystem _light = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _link = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly SharedEntityEffectsSystem _effects = default!;
    
    /// <summary>
    /// Psychological soothing is used to increase the maximum temperature at which the supermatter can operate without overheating, as well as decreasing waste production.
    /// </summary>
    private EntityQuery<PsychologicalSoothingReceiverComponent> _psyReceiversQuery = default!;
    
    /// <summary>
    /// This is used for speech sounds
    /// TODO: This can probably be moved to a shared system?
    /// </summary>
    private EntityQuery<SpeechComponent> _speechQuery = default!;
    
    /// <summary>
    /// This is used for ambient sounds.
    /// TODO: This can probably be moved to a shared system?
    /// </summary>
    private EntityQuery<AmbientSoundComponent> _ambientQuery = default!;

    /// <summary>
    /// Controls the appearance of the supermatter.
    /// TODO: This can probably be moved to a shared system?
    /// </summary>
    private EntityQuery<AppearanceComponent> _appearanceQuery = default!;
    
    /// <summary>
    /// This is used for the gravitational disturbances produced by the supermatter.
    /// </summary>
    private EntityQuery<GravityWellComponent> _gravityWellQuery = default!;
    
    /// <summary>
    /// This is used for the radiation levels produced by the supermatter.
    /// </summary>
    private EntityQuery<RadiationSourceComponent> _radiationSourceQuery = default!;
    
    /// <summary>
    /// This is used for device linking and signals.
    /// </summary>
    private EntityQuery<DeviceLinkSourceComponent> _linkQuery = default!;
    
    /// <summary>
    /// This is used to determine if an entity is immune to being consumed by the supermatter.
    /// </summary>
    private EntityQuery<SupermatterImmuneComponent> _immuneQuery = default!;
    
    /// <summary>
    /// This is used to consume both the entity and the item if an otherwise undroppable item is used on the supermatter.
    /// </summary>
    private EntityQuery<UnremoveableComponent> _unremoveableQuery = default!;
    
    /// <summary>
    /// This is used to convert projectile damage into supermatter power.
    /// </summary>
    private EntityQuery<ProjectileComponent> _projectileQuery = default!;
    
    /// <summary>
    /// This is used for consuming mobs bumping into the supermatter
    /// </summary>
    private EntityQuery<MobStateComponent> _mobStateQuery = default!;
    
    /// <summary>
    /// This is used to let ghosts see the integrity of the supermatter, and to make sure we don't consume any items held by ghosts.
    /// </summary>
    private EntityQuery<GhostComponent> _ghostQuery = default!;
    
    /// <summary>
    /// This is used to avoid consuming entities with godmode.
    /// </summary>
    private EntityQuery<GodmodeComponent> _godmodeQuery = default!;
    
    /// <summary>
    /// This is used to get the mass of items touching the supermatter.
    /// </summary>
    private EntityQuery<PhysicsComponent> _physicsQuery = default!;
    
    /// <summary>
    /// This is used to get the energy value of items tagged as supermatter food.
    /// </summary>
    private EntityQuery<SupermatterFoodComponent> _foodQuery = default!;
    
    protected override string SawmillName => "supermatter";

    public override void Initialize()
    {
        base.Initialize();

        _psyReceiversQuery = GetEntityQuery<PsychologicalSoothingReceiverComponent>();
        _speechQuery = GetEntityQuery<SpeechComponent>();
        _appearanceQuery = GetEntityQuery<AppearanceComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _gravityWellQuery = GetEntityQuery<GravityWellComponent>();
        _foodQuery = GetEntityQuery<SupermatterFoodComponent>();
        _linkQuery = GetEntityQuery<DeviceLinkSourceComponent>();
        _immuneQuery = GetEntityQuery<SupermatterImmuneComponent>();
        _godmodeQuery = GetEntityQuery<GodmodeComponent>();
        _unremoveableQuery = GetEntityQuery<UnremoveableComponent>();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _ghostQuery = GetEntityQuery<GhostComponent>();
        _ambientQuery = GetEntityQuery<AmbientSoundComponent>();
        _radiationSourceQuery = GetEntityQuery<RadiationSourceComponent>();

        SubscribeLocalEvent<SupermatterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SupermatterComponent, AtmosDeviceUpdateEvent>(OnSupermatterUpdated);

        SubscribeLocalEvent<SupermatterComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<SupermatterComponent, EmbeddedEvent>(OnEmbedded);
        SubscribeLocalEvent<SupermatterComponent, InteractHandEvent>(OnHandInteract);
        SubscribeLocalEvent<SupermatterComponent, InteractUsingEvent>(OnItemInteract);
        SubscribeLocalEvent<SupermatterComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDoAfterEvent>(OnGetSliver);
        SubscribeLocalEvent<SupermatterComponent, GravPulseEvent>(OnGravPulse);
        
        SubscribeLocalEvent<SupermatterComponent, SupermatterDamagedEvent>(OnSupermatterDamaged);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDelaminationStartedEvent>(OnSupermatterDelaminationStarted);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDelaminationCancelledEvent>(OnSupermatterDelaminationCancelled);
        SubscribeLocalEvent<SupermatterComponent, SupermatterStatusChangedEvent>(OnSupermatterStatusChanged);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDelaminationEvent>(OnSupermatterDelamination);
        SubscribeLocalEvent<SupermatterComponent, SupermatterAnnouncementEvent>(OnSupermatterAnnouncement);
    }
    
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SupermatterComponent>();
        
        while (query.MoveNext(out var uid, out var sm))
        {
            if (sm.DelaminationTime.HasValue && sm.DelaminationTime <= _timing.CurTime)
            {
                var ev = new SupermatterDelaminationEvent();
                RaiseLocalEvent(uid, ref ev, true);
            }
            
            if(sm.AnnounceNext.HasValue && sm.AnnounceNext.Value <= _timing.CurTime)
            {
                SetNextAnnouncementTime(sm);
                
                var ev = new SupermatterAnnouncementEvent();
                RaiseLocalEvent(uid, ref ev, true);
            }
            
            if(_psyReceiversQuery.TryComp(uid, out var psyReceiver) && _appearanceQuery.TryComp(uid, out var appearance))
                _appearance.SetData(uid, SupermatterVisuals.Psy, psyReceiver.SoothedCurrent, appearance);
        }
    }

    private void OnMapInit(EntityUid uid, SupermatterComponent sm, MapInitEvent args)
    {
        // Set the sound
        _ambient.SetAmbience(uid, true);

        // Send the inactive port for any linked devices
        if (_linkQuery.HasComp(uid))
            _link.InvokePort(uid, sm.PortInactive);
    }
    
    private void OnSupermatterAnnouncement(EntityUid uid, SupermatterComponent sm, SupermatterAnnouncementEvent args)
    {
        if (sm.SuppressAnnouncements)
            return;
        
        // We do not need to send any announcements if the supermatter is not damaged.
        if (MathHelper.CloseTo(sm.Damage, 0.0f, 0.05f))
            return;
        
        var integrity = GetIntegrity(sm).ToString("0.00");
        var isHealing = sm.Damage < sm.DamageArchived;
        var isTakingDamage = sm.Damage > sm.DamageArchived;
        
        switch (sm.Status)
        {
            case SupermatterStatusType.Delaminating when sm.IsDelaminationAnnounced && sm.DelaminationTime.HasValue:
            {
                var seconds = Math.Ceiling(sm.DelaminationTime.Value.TotalSeconds - _timing.CurTime.TotalSeconds);
                
                var message = seconds switch
                {
                    > 60 => Loc.GetString("supermatter-time-before-delam", ("time", sm.DelaminationTime.Value)),
                    < 5 => Loc.GetString("supermatter-seconds-before-delam-imminent", ("seconds", seconds)),
                    _ => Loc.GetString("supermatter-seconds-before-delam-countdown", ("seconds", seconds)),
                };
                
                if (seconds < 5 && _speechQuery.TryComp(uid, out var speech))
                    speech.SoundCooldownTime = 4.5f;
            
                SendSupermatterAnnouncement(uid, sm, message, true);
                break;
            }
            case >= SupermatterStatusType.Warning when isHealing:
            {
                var message = Loc.GetString("supermatter-healing", ("integrity", integrity));
                var global = sm.Status >= SupermatterStatusType.Emergency;

                if (_speechQuery.TryComp(uid, out var speech))
                    // Reset speech cooldown after healing is started
                    speech.SoundCooldownTime = 0.0f;
            
                SendSupermatterAnnouncement(uid, sm, message, global);
                break;
            }
            case >= SupermatterStatusType.Warning when isTakingDamage && !sm.IsDelaminating:
            {
                // We don't want to send the 0% integrity message, and we only want to emit the warning if the supermatter is taking damage. 
                var isEmergency = sm.Damage >= sm.DamageEmergencyThreshold;
                var message = Loc.GetString( isEmergency? "supermatter-emergency" : "supermatter-warning", ("integrity", integrity));
                SendSupermatterAnnouncement(uid, sm, message, isEmergency);

                if (sm.Power >= _config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold))
                {
                    SendSupermatterAnnouncement(uid, sm, Loc.GetString(sm.PowerlossInhibitor >= 0.5 ? "supermatter-threshold-power" : "supermatter-threshold-powerloss"));
                }
                
                if (sm.GasStorage != null && sm.GasStorage.TotalMoles >= _config.GetCVar(ImpCCVars.SupermatterMolePenaltyThreshold))
                {
                    message = Loc.GetString("supermatter-threshold-mole");
                    SendSupermatterAnnouncement(uid, sm, message);
                }
                
                break;
            }
        }
    }

    private void OnSupermatterUpdated(EntityUid uid, SupermatterComponent sm, AtmosDeviceUpdateEvent args)
    {
        ProcessAtmos(uid, sm, args.dt);
        HandleDamage(uid, sm);
        
        UpdateSupermatterStatus((uid, sm));
        
        HandleLight(uid, sm);
        HandleVision(uid, sm);
        HandleAccent(uid, sm);

        if (sm.Power > _config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold) || sm.Damage > sm.DamagePenaltyPoint)
        {
            GenerateAnomalies(uid, sm);
        }
    }
    
    private void OnSupermatterDamaged(EntityUid uid, SupermatterComponent sm, SupermatterDamagedEvent args)
    {   
        if (sm.Damage >= sm.DamageDelaminationThreshold && !sm.IsDelaminating)
        {
            // Start the delamination process
            sm.IsDelaminating = true;
            sm.DelaminationTime = _timing.CurTime + sm.DelaminationDelay;

            var ev = new SupermatterDelaminationStartedEvent();
            RaiseLocalEvent(uid, ref ev);
        }
        else if (sm.Damage < sm.DamageDelaminationThreshold && sm.IsDelaminating)
        {
            // Cancel the delamination process
            sm.IsDelaminating = false;
            sm.DelaminationTime = null;

            var ev = new SupermatterDelaminationCancelledEvent();
            RaiseLocalEvent(uid, ref ev);
        }
    }
    
    private void OnSupermatterStatusChanged(EntityUid uid, SupermatterComponent sm, SupermatterStatusChangedEvent args)
    {
        _adminLog.Add(LogType.Unknown, LogImpact.Medium, $"{EntityManager.ToPrettyString(uid):uid} status changed to {sm.Status}");
        
        // Adjust the supermatter's sprite
        if (_appearanceQuery.TryComp(uid, out var appearance))
        {
            var visual = SupermatterCrystalState.Normal;
            if (sm.Damage > 0 && sm.Damage > sm.DamageArchived) // Damaged and not healing
            {
                visual = sm.Status switch
                {
                    SupermatterStatusType.Delaminating => SupermatterCrystalState.GlowDelam,
                    >= SupermatterStatusType.Emergency => SupermatterCrystalState.GlowEmergency,
                    _ => SupermatterCrystalState.Glow
                };
            }

            _appearance.SetData(uid, SupermatterVisuals.Crystal, visual, appearance);
        }

        // Update linked devices
        if (_linkQuery.HasComp(uid))
        {
            var port = sm.Status switch
            {
                SupermatterStatusType.Normal => sm.PortNormal,
                SupermatterStatusType.Caution => sm.PortCaution,
                SupermatterStatusType.Warning => sm.PortWarning,
                SupermatterStatusType.Danger => sm.PortDanger,
                SupermatterStatusType.Emergency => sm.PortEmergency,
                SupermatterStatusType.Delaminating => sm.PortDelaminating,
                _ => sm.PortInactive
            };

            _link.InvokePort(uid, port);
        }
        
        // Update speech sounds
        if (_speechQuery.TryComp(uid, out var speech))
        {
            speech.SpeechSounds = sm.Status switch
            {
                < SupermatterStatusType.Delaminating when sm.Damage < sm.DamageArchived => sm.StatusSilentSound,
                
                SupermatterStatusType.Warning => sm.StatusWarningSound,
                SupermatterStatusType.Danger => sm.StatusDangerSound,
                SupermatterStatusType.Emergency => sm.StatusEmergencySound,
                SupermatterStatusType.Delaminating => sm.StatusDelamSound,
                
                _ => sm.StatusSilentSound
            };
        }

        if (_ambientQuery.TryComp(uid, out var ambient))
        {

            var volume = (float) Math.Round(Math.Clamp(sm.Power / 50 - 5, -5, 5));

            _ambient.SetVolume(uid, volume);

            switch (sm.Status)
            {
                case >= SupermatterStatusType.Danger when ambient.Sound != sm.DelamLoopSound:
                    _ambient.SetSound(uid, sm.DelamLoopSound, ambient);
                    break;
                case < SupermatterStatusType.Danger when ambient.Sound != sm.CalmLoopSound:
                    _ambient.SetSound(uid, sm.CalmLoopSound, ambient);
                    break;
            }
        }

        // We should give the supermatter a chance to announce a few seconds after the status changes.
        // Only do this for less than delaminating status so we don't clobber the ominous countdown.
        if(sm.Status < SupermatterStatusType.Delaminating)
            SetNextAnnouncementTime(sm, TimeSpan.FromSeconds(5));
    }
    
    private void OnSupermatterDelaminationStarted(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationStartedEvent args)
    {
        var sb = new StringBuilder();
        sm.PreferredDelamination ??= ChooseDelamType(uid, sm);
        _adminLog.Add(LogType.Unknown, LogImpact.Medium, $"{EntityManager.ToPrettyString(uid):uid} delamination started with type {sm.PreferredDelamination?.ID ?? "None"}");
        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} delamination started with type {sm.PreferredDelamination?.ID ?? "None"}");

        sb.AppendLine(Loc.GetString(sm.PreferredDelamination?.Message ?? "supermatter-delam-generic"));
        sb.Append(Loc.GetString("supermatter-time-before-delam", ("time", sm.DelaminationDelay)));

        sm.IsDelaminationAnnounced = true;
        SendSupermatterAnnouncement(uid, sm, sb.ToString(), true);
        
        SetNextAnnouncementTime(sm);
    }
    
    private void OnSupermatterDelaminationCancelled(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationCancelledEvent args)
    {
        _adminLog.Add(LogType.Unknown, LogImpact.Medium, $"{EntityManager.ToPrettyString(uid):uid} delamination cancelled");

        sm.IsDelaminationAnnounced = false;
        sm.PreferredDelamination = null;
        
        var integrity = GetIntegrity(sm).ToString("0.00");
        SendSupermatterAnnouncement(uid, sm, Loc.GetString("supermatter-delam-cancel", ("integrity", integrity)), true);
    }
    
    private void OnSupermatterDelamination(EntityUid uid, SupermatterComponent sm, SupermatterDelaminationEvent args)
    {
        if (sm.PreferredDelamination is null)
        {
            _adminLog.Add(LogType.Unknown, LogImpact.Extreme, $"{EntityManager.ToPrettyString(uid):uid} failed to choose a delamination type and was deleted at {Transform(uid).Coordinates:coordinates}");
            _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} failed to choose a delamination type and was deleted");
            
            // No delamination type was chosen, and no default was specified. Just delete the supermatter.
            QueueDel(uid);
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
        _audio.PlayGlobal(sm.DistortSound, mapFilter, true);
        
        // TODO: Move this to a GameRule
        // Flickers all powered lights on the map
        var lightLookup = new HashSet<Entity<PoweredLightComponent>>();
        _entityLookup.GetEntitiesOnMap<PoweredLightComponent>(mapId, lightLookup);
        foreach (var light in lightLookup)
        {
            if (!_random.Prob(sm.LightFlickerChance))
                continue;
            _ghost.DoGhostBooEvent(light);
        }
        
        foreach (var gameRule in sm.PreferredDelamination.GameRules)
        {
            // delamination game rules
            var gameRuleEnt = _gameTicker.AddGameRule(gameRule);
            _gameTicker.StartGameRule(gameRuleEnt);
        }
        
        // Give effects to every mob on the map, except those in EntityStorage (lockers, etc)
        var mobLookup = new HashSet<Entity<MobStateComponent>>();
        _entityLookup.GetEntitiesOnMap(mapId, mobLookup);
        var insideEntityStorageQuery = GetEntityQuery<InsideEntityStorageComponent>();
        mobLookup.RemoveWhere(x => insideEntityStorageQuery.HasComp(x));
        
        var effects = _proto.Index(sm.DelamEffectsPrototype).Components;
        foreach (var mob in mobLookup)
        {
            _effects.ApplyEffects(mob, sm.PreferredDelamination.MobEffects);
            
            // Add effects to all mobs
            // TODO: change paracusia to actual hallucinations whenever those are real
            EntityManager.AddComponents(mob, effects, false);
        }
        
        _effects.ApplyEffects(uid, sm.PreferredDelamination.SupermatterEffects);
        
        // Not every delamination will automatically destroy the supermatter.
        // So we're going to queue it for deletion just to be sure.
        QueueDel(uid);
    }

    private void OnCollideEvent(EntityUid uid, SupermatterComponent sm, ref StartCollideEvent args)
    {
        TryCollision(uid, sm, args.OtherEntity, args.OtherBody);
    }

    private void OnEmbedded(EntityUid uid, SupermatterComponent sm, ref EmbeddedEvent args)
    {
        TryCollision(uid, sm, args.Embedded, checkStatic: false);
    }

    private void OnHandInteract(EntityUid uid, SupermatterComponent sm, ref InteractHandEvent args)
    {
        var target = args.User;

        if (_immuneQuery.HasComp(target) || _godmodeQuery.HasComp(target))
            return;

        if (!sm.HasBeenPowered)
            LogFirstPower(uid, sm, target);

        var power = 200f;
        
        if (_physicsQuery.TryComp(target, out var physics))
            power += physics.Mass;

        sm.MatterPower += power;

        _popup.PopupEntity(Loc.GetString("supermatter-collide-mob", ("sm", uid), ("target", target)), uid, PopupType.LargeCaution);
        _audio.PlayPvs(sm.DustSound, uid);

        // Prevent spam or excess power production
        AddComp<SupermatterImmuneComponent>(target);

        _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} has consumed {EntityManager.ToPrettyString(target):target}");
        _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} touched {EntityManager.ToPrettyString(uid):uid} and was destroyed at {Transform(uid).Coordinates:coordinates}");
        EntityManager.SpawnEntity(sm.CollisionResultPrototype, Transform(target).Coordinates);
        EntityManager.QueueDeleteEntity(target);

        args.Handled = true;
    }

    private void OnItemInteract(EntityUid uid, SupermatterComponent sm, ref InteractUsingEvent args)
    {
        var target = args.User;
        var item = args.Used;
        var othersFilter = Filter.Pvs(uid).RemovePlayerByAttachedEntity(target);

        if (args.Handled ||
            _ghostQuery.HasComp(target) ||
            _immuneQuery.HasComp(item) ||
            _godmodeQuery.HasComp(item))
            return;

        // TODO: supermatter scalpel
        if (_unremoveableQuery.HasComp(item))
        {
            if (!sm.HasBeenPowered)
                LogFirstPower(uid, sm, target);

            var power = 200f;

            if (_physicsQuery.TryComp(target, out var targetPhysics))
                power += targetPhysics.Mass;

            if (_physicsQuery.TryComp(item, out var itemPhysics))
                power += itemPhysics.Mass;

            sm.MatterPower += power;

            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert-unremoveable", ("target", target), ("sm", uid), ("item", item)), uid, othersFilter, true, PopupType.LargeCaution);
            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert-unremoveable-user", ("sm", uid), ("item", item)), uid, target, PopupType.LargeCaution);
            _audio.PlayPvs(sm.DustSound, uid);

            // Prevent spam or excess power production
            AddComp<SupermatterImmuneComponent>(target);
            AddComp<SupermatterImmuneComponent>(item);

            _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} touched {EntityManager.ToPrettyString(uid):uid} with {EntityManager.ToPrettyString(item):item} and both were destroyed at {Transform(uid).Coordinates:coordinates}");
            EntityManager.SpawnEntity(sm.CollisionResultPrototype, Transform(target).Coordinates);
            EntityManager.QueueDeleteEntity(target);
            EntityManager.QueueDeleteEntity(item);
        }
        else
        {
            if (!sm.HasBeenPowered)
                LogFirstPower(uid, sm, item);

            if (_physicsQuery.TryComp(item, out var physics))
                sm.MatterPower += physics.Mass;

            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert", ("target", target), ("sm", uid), ("item", item)), uid, othersFilter, true, PopupType.LargeCaution);
            _popup.PopupEntity(Loc.GetString("supermatter-collide-insert-user", ("sm", uid), ("item", item)), uid, target, PopupType.LargeCaution);
            _audio.PlayPvs(sm.DustSound, uid);

            // Prevent spam or excess power production
            AddComp<SupermatterImmuneComponent>(item);

            _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} touched {EntityManager.ToPrettyString(uid):uid} with {EntityManager.ToPrettyString(item):item} and destroyed it at {Transform(uid).Coordinates:coordinates}");
            EntityManager.QueueDeleteEntity(item);
        }

        args.Handled = true;
    }

    private void OnGetSliver(EntityUid uid, SupermatterComponent sm, ref SupermatterDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        // Your criminal actions will not go unnoticed
        sm.Damage += sm.DamageDelaminationThreshold / 10.0f;

        var integrity = GetIntegrity(sm).ToString("0.00");
        SendSupermatterAnnouncement(uid, sm, Loc.GetString("supermatter-announcement-cc-tamper", ("integrity", integrity)));

        Spawn(sm.SliverPrototype, Transform(args.User).Coordinates);
        _popup.PopupClient(Loc.GetString("supermatter-tamper-end"), uid, args.User);

        sm.DelaminationDelay /= 2;
    }

    private void OnGravPulse(Entity<SupermatterComponent> ent, ref GravPulseEvent args)
    {
        if (!_gravityWellQuery.TryComp(ent, out var gravityWell))
            return;

        var nextPulse = 0.5f * _random.NextFloat(1f, 30f);
        _gravityWell.SetPulsePeriod(ent, TimeSpan.FromSeconds(nextPulse), gravityWell);

        var audioParams = AudioParams.Default.WithMaxDistance(gravityWell.MaxRange);
        _audio.PlayPvs(ent.Comp.PullSound, ent, audioParams);
    }

    private void OnExamine(EntityUid uid, SupermatterComponent sm, ref ExaminedEvent args)
    {
        // For ghosts: alive players can use the console
        if (_ghostQuery.HasComp(args.Examiner) && args.IsInDetailsRange)
            args.PushMarkup(Loc.GetString("supermatter-examine-integrity", ("integrity", GetIntegrity(sm).ToString("0.00"))));
    }

    private void TryCollision(EntityUid uid, SupermatterComponent sm, EntityUid target, PhysicsComponent? targetPhysics = null, bool checkStatic = true)
    {
        if (!Resolve(target, ref targetPhysics))
            return;

        if (targetPhysics.BodyType == BodyType.Static && checkStatic ||
            _immuneQuery.HasComp(target) ||
            _godmodeQuery.HasComp(target) ||
            _container.IsEntityInContainer(uid))
            return;

        if (!sm.HasBeenPowered)
            LogFirstPower(uid, sm, target);

        if (!_projectileQuery.HasComp(target))
        {
            var popup = "supermatter-collide";

            if (_mobStateQuery.HasComp(target))
            {
                popup = "supermatter-collide-mob";
                EntityManager.SpawnEntity(sm.CollisionResultPrototype, Transform(target).Coordinates);
                _chatManager.SendAdminAlert($"{EntityManager.ToPrettyString(uid):uid} has consumed {EntityManager.ToPrettyString(target):target}");
            }

            var targetProto = MetaData(target).EntityPrototype;
            if (targetProto != null && targetProto.ID != sm.CollisionResultPrototype)
            {
                _popup.PopupEntity(Loc.GetString(popup, ("sm", uid), ("target", target)), uid, PopupType.LargeCaution);
                _audio.PlayPvs(sm.DustSound, uid);
            }

            sm.MatterPower += targetPhysics.Mass;
            _adminLog.Add(LogType.EntityDelete, LogImpact.High, $"{EntityManager.ToPrettyString(target):target} collided with {EntityManager.ToPrettyString(uid):uid} at {Transform(uid).Coordinates:coordinates}");
        }

        // Prevent spam or excess power production
        AddComp<SupermatterImmuneComponent>(target);

        EntityManager.QueueDeleteEntity(target);

        if (_foodQuery.TryComp(target, out var food))
            sm.Power += food.Energy;
        else if (_projectileQuery.TryComp(target, out var projectile))
            sm.Power += (float)projectile.Damage.GetTotal();
        else
            sm.Power++;

        sm.MatterPower += _mobStateQuery.HasComp(target) ? 200 : 0;
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

    private SupermatterStatusType GetStatus(Entity<SupermatterComponent> ent)
    {
        var mix = _atmosphere.GetContainingMixture(ent.Owner, true, true);

        if (mix is not { })
            return SupermatterStatusType.Error;

        if (ent.Comp.IsDelaminating || ent.Comp.Damage >= ent.Comp.DamageDelaminationThreshold)
            return SupermatterStatusType.Delaminating;

        if (ent.Comp.Damage >= ent.Comp.DamageEmergencyThreshold)
            return SupermatterStatusType.Emergency;

        if (ent.Comp.Damage >= ent.Comp.DamageDangerThreshold)
            return SupermatterStatusType.Danger;

        if (ent.Comp.Damage >= ent.Comp.DamageWarningThreshold)
            return SupermatterStatusType.Warning;

        if (mix.Temperature > Atmospherics.T0C + _config.GetCVar(ImpCCVars.SupermatterHeatPenaltyThreshold) * 0.8)
            return SupermatterStatusType.Caution;

        if (ent.Comp.Power > 5)
            return SupermatterStatusType.Normal;

        return SupermatterStatusType.Inactive;
    }

    private bool CheckDelaminationRequirements(SupermatterDelaminationRequirements req, GasMixture? mix, SupermatterComponent sm)
    {
        if (req.MinPower.HasValue && sm.Power < req.MinPower.Value)
            return false;
        
        if (req.MaxPower.HasValue && sm.Power > req.MaxPower.Value)
            return false;

        var absorbedMoles = mix is null ? 0 : mix.TotalMoles * GetGasEfficiency(sm);
        
        if (req.MinMoles.HasValue && absorbedMoles < req.MinMoles.Value)
            return false;
        
        if (req.MaxMoles.HasValue && absorbedMoles > req.MaxMoles.Value)
            return false;
        
        if (req.MinGlimmer.HasValue && _glimmer.Glimmer < req.MinGlimmer.Value)
            return false;
        
        if (req.MaxGlimmer.HasValue && _glimmer.Glimmer > req.MaxGlimmer.Value)
            return false;
        
        return true;
    }
    
    private TimeSpan GetAnnouncementDelay(SupermatterComponent sm)
    {
        if (sm.IsDelaminating && sm.DelaminationTime.HasValue)
        {
            return (sm.DelaminationTime.Value.TotalSeconds - _timing.CurTime.TotalSeconds) switch
            {
                > 30 => TimeSpan.FromSeconds(10),
                > 5 => TimeSpan.FromSeconds(5),
                <= 5 => TimeSpan.FromSeconds(1),
                _ => TimeSpan.FromSeconds(10)
            };
        }

        return sm.AnnounceInterval.TotalSeconds >= 1.0 ? sm.AnnounceInterval : TimeSpan.FromSeconds(1);

    }

    public void SetNextAnnouncementTime(SupermatterComponent sm, TimeSpan delay)
    {
        sm.AnnounceNext = _timing.CurTime + delay ;
    }

    public void SetNextAnnouncementTime(SupermatterComponent sm)
    {
        sm.AnnounceNext = _timing.CurTime + GetAnnouncementDelay(sm);
    }
    
    /// <summary>
    /// Checks the supermatter's status and updates it accordingly, then raises a status changed event if it has changed.
    /// </summary>
    private void UpdateSupermatterStatus(Entity<SupermatterComponent> ent)
    {
        var currentStatus = GetStatus(ent);

        // Send port updates out for any linked devices
        if (ent.Comp.Status != currentStatus)
        {
            ent.Comp.Status = currentStatus;
            var ev = new SupermatterStatusChangedEvent();
            RaiseLocalEvent(ent, ref ev);
        }
    }
}
