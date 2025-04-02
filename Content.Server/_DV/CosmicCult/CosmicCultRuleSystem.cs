using Content.Server._DV.CosmicCult.Components;
using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.Atmos.Components;
using Content.Server.Audio;
using Content.Server.Bible.Components;
using Content.Server.Chat.Systems;
using Content.Server.CrewManifest;
using Content.Server.EUI;
using Content.Server.GameTicking.Events;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Objectives.Components;
using Content.Server.Popups;
using Content.Server.Radio.Components;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Voting.Managers;
using Content.Server.Voting;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Alert;
using Content.Shared.Audio;
using Content.Shared.Body.Systems;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Parallax;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Stunnable;
using Content.Shared.Temperature.Components;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._DV.CosmicCult;

/// <summary>
/// Where all the main stuff for Cosmic Cultists happens.
/// </summary>
public sealed class CosmicCultRuleSystem : GameRuleSystem<CosmicCultRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EuiManager _euiMan = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IVoteManager _votes = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly CosmicCorruptingSystem _corrupting = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    private ISawmill _sawmill = default!;

    private readonly SoundSpecifier _briefingSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/antag_cosmic_briefing.ogg");
    private readonly SoundSpecifier _deconvertSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/antag_cosmic_deconvert.ogg");
    private readonly SoundSpecifier _tier3Sound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/tier3.ogg");
    private readonly SoundSpecifier _tier2Sound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/tier2.ogg");
    private readonly SoundSpecifier _monumentAlert = new SoundPathSpecifier("/Audio/_DV/CosmicCult/tier_up.ogg");

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("cosmiccult");

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<CosmicCultAssociateRuleEvent>(OnAssociateRule);

        SubscribeLocalEvent<CosmicCultRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);

        SubscribeLocalEvent<CosmicCultComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<CosmicMarkGodComponent, ComponentInit>(OnGodSpawn);
        SubscribeLocalEvent<CosmicCultComponent, MobStateChangedEvent>(OnMobStateChanged);
    }
    #region Starting Events
    protected override void Started(EntityUid uid, CosmicCultRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        Timer.Spawn(TimeSpan.FromSeconds(10), () => { StewardVote(); });
    }

    private void StewardVote()
    {
        var cultists = new Dictionary<string, EntityUid>();

        var cultQuery = EntityQueryEnumerator<CosmicCultComponent>();
        while (cultQuery.MoveNext(out var cult, out _))
        {
            var playerInfo = $"{Comp<MetaDataComponent>(cult).EntityName}";
            cultists.Add(playerInfo, cult);
        }

        var options = new VoteOptions
        {
            DisplayVotes = false,
            Title = Loc.GetString("cosmiccult-vote-steward-title"),
            InitiatorText = Loc.GetString("cosmiccult-vote-steward-initiator"),
            Duration = TimeSpan.FromSeconds(_config.GetCVar(DCCVars.CosmicCultStewardVoteTimer)),
            VoterEligibility = VoteManager.VoterEligibility.CosmicCult
        };

        foreach (var (name, ent) in cultists)
        {
            options.Options.Add((Loc.GetString(name), ent));
        }

        var vote = _votes.CreateVote(options);

        vote.OnFinished += (_, args) =>
        {
            EntityUid picked;
            if (args.Winner == null)
            {
                picked = (EntityUid)_rand.Pick(args.Winners);
            }
            else
            {
                picked = (EntityUid)args.Winner;
            }
            AddComp<CosmicCultLeadComponent>(picked);
            _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Cult stewardship vote finished: {Identity.Entity(picked, EntityManager)} is now steward.");
            _antag.SendBriefing(picked, Loc.GetString("cosmiccult-vote-steward-briefing"), Color.FromHex("#4cabb3"), _monumentAlert);
        };
    }

    private void OnAntagSelect(Entity<CosmicCultRuleComponent> uid, ref AfterAntagEntitySelectedEvent args)
    {
        TryStartCult(args.EntityUid, uid);
    }
    #endregion

    #region Round & Objectives

    private void OnGodSpawn(Entity<CosmicMarkGodComponent> uid, ref ComponentInit args)
    {
        var query = QueryActiveRules();

        while (query.MoveNext(out var ruleUid, out _, out var cultRule, out _))
        {
            SetWinType((ruleUid, cultRule), WinType.CultComplete); //here's no coming back from this. Cult wins this round
            _roundEnd.EndRound(); //Woo game over yeaaaah
            foreach (var cultist in cultRule.Cultists)
            {
                if (TryComp<MobStateComponent>(cultist, out var state) && state.CurrentState != MobState.Dead)
                {
                    if (!TryComp<MindContainerComponent>(cultist, out var mindContainer) || !mindContainer.HasMind)
                        return;

                    var ascendant = Spawn("MobCosmicAstralAscended", Transform(cultist).Coordinates);
                    _mind.TransferTo(mindContainer.Mind.Value, ascendant);
                    _metaData.SetEntityName(ascendant, Loc.GetString("cosmiccult-astral-ascendant", ("name", cultist))); //Renames cultists' ascendant forms to "[CharacterName], Ascendant"
                    _body.GibBody(cultist); // you don't need that body anymore
                }
            }
            QueueDel(cultRule.MonumentInGame); // The monument doesn't need to stick around postround! Into the bin with you.
            QueueDel(cultRule.MonumentSlowZone); // cease exist
        }
    }

    private static void SetWinType(Entity<CosmicCultRuleComponent> ent, WinType type)
    {
        if (ent.Comp.WinLocked)
            return;
        ent.Comp.WinType = type;

        if (type is WinType.CultComplete or WinType.CrewComplete) //Let's lock in our WinType to prevent us from setting a worse win if a better win's been achieved.
            ent.Comp.WinLocked = true;
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New is not GameRunLevel.PostRound) //Are we moving to post-round?
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var cultRule, out _))
        {
            ConfirmWinState((uid, cultRule)); //If so, let's consult our Winconditions and set an appropriate WinType.
        }
    }

    private bool CultistsAlive()
    {
        var query = EntityQueryEnumerator<CosmicCultComponent, MobStateComponent>();
        while (query.MoveNext(out _, out var comp, out var mob))
        {
            if (mob.Running && mob.CurrentState == MobState.Alive)
                return true;
        }
        
        return false;
    }

    private void OnMobStateChanged(Entity<CosmicCultComponent> ent, ref MobStateChangedEvent args)
    {
        if (CultistsAlive())
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var ruleUid, out _, out var ruleComp, out _))
        {
            ConfirmWinState((ruleUid, ruleComp));
        }
    }

    private void ConfirmWinState(Entity<CosmicCultRuleComponent> ent)
    {
        var tier = ent.Comp.CurrentTier;
        var leaderAlive = false;
        var centcomm = _emergency.GetCentcommMaps();
        var wrapup = AllEntityQuery<CosmicCultComponent, TransformComponent>();
        while (wrapup.MoveNext(out var cultist, out _, out var cultistLocation))
        {
            if (cultistLocation.MapUid != null && centcomm.Contains(cultistLocation.MapUid.Value))
            {
                if (HasComp<CosmicCultLeadComponent>(cultist))
                    leaderAlive = true;
            }
        }
        if (tier < 3 && leaderAlive)
            SetWinType(ent, WinType.Neutral); //The Monument isn't Tier 3, but the cult leader's alive and at Centcomm! a Neutral outcome
        var monument = AllEntityQuery<CosmicFinaleComponent>();
        while (monument.MoveNext(out var monumentUid, out var comp))
        {
            _sound.StopStationEventMusic(ent, StationEventMusicType.CosmicCult);
            if (tier == 3 && comp.CurrentState == FinaleState.Unavailable)
            {
                SetWinType(ent, WinType.CultMinor); //The crew escaped, and The Monument wasn't fully empowered. a small win
            }
            else if (comp.CurrentState != FinaleState.Unavailable)
            {
                SetWinType(ent, WinType.CultMajor); //Despite the crew's escape, The Finale is available or active. Major win
            }
        }

        if (CultistsAlive())
            return; // There's still cultists alive! stop checking stuff

        _roundEnd.DoRoundEndBehavior(ent.Comp.RoundEndBehavior, ent.Comp.EvacShuttleTime, ent.Comp.RoundEndTextSender, ent.Comp.RoundEndTextShuttleCall, ent.Comp.RoundEndTextAnnouncement);
        ent.Comp.RoundEndBehavior = RoundEndBehavior.Nothing; // prevent this being called multiple times.

        var gameruleMonument = ent.Comp.MonumentInGame;
        if (TryComp<CosmicFinaleComponent>(gameruleMonument, out var finComp))
        {
            gameruleMonument.Comp.Enabled = false;
            finComp.CurrentState = FinaleState.Unavailable;
            _popup.PopupCoordinates(Loc.GetString("cosmiccult-monument-powerdown"), Transform(gameruleMonument).Coordinates, PopupType.Large);
            _sound.StopStationEventMusic(gameruleMonument, StationEventMusicType.CosmicCult);
            UpdateMonumentAppearance(gameruleMonument, false);
        }

        if (ent.Comp.TotalCult == 0)
            SetWinType(ent, WinType.CrewComplete); // No cultists registered! That means everyone got deconverted
        else
            SetWinType(ent, WinType.CrewMajor); // There's still cultists registered, but if we got here, that means they're all dead
    }

    protected override void AppendRoundEndText(EntityUid uid,
        CosmicCultRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        var ftlKey = component.WinType.ToString().ToLower();
        var winType = Loc.GetString($"cosmiccult-roundend-{ftlKey}");
        var summaryText = Loc.GetString($"cosmiccult-summary-{ftlKey}");
        args.AddLine(winType);
        args.AddLine(summaryText);
        args.AddLine(Loc.GetString("cosmiccult-roundend-cultist-count", ("initialCount", component.TotalCult)));
        args.AddLine(Loc.GetString("cosmiccult-roundend-cultpop-count", ("count", component.PercentConverted)));
        args.AddLine(Loc.GetString("cosmiccult-roundend-entropy-count", ("count", component.EntropySiphoned)));
        args.AddLine(Loc.GetString("cosmiccult-roundend-monument-stage", ("stage", component.CurrentTier)));
    }

    public void IncrementCultObjectiveEntropy(Entity<CosmicCultComponent> ent)
    {
        if (AssociatedGamerule(ent) is not {} cult)
            return;

        cult.Comp.EntropySiphoned += ent.Comp.CosmicSiphonQuantity;
        var query = EntityQueryEnumerator<CosmicEntropyConditionComponent>();
        while (query.MoveNext(out _, out var entropyComp))
        {
            entropyComp.Siphoned = cult.Comp.EntropySiphoned;
        }
    }
    #endregion

    #region Monument
    public void UpdateMonumentAppearance(Entity<MonumentComponent> ent, bool tierUp) // this is kinda awful, but it works, and i've seen worse. improve it at thine leisure
    {
        if (AssociatedGamerule(ent) is not {} cult)
            return;
        if (!TryComp<CosmicFinaleComponent>(ent, out var finaleComp))
            return;
        _appearance.SetData(ent, MonumentVisuals.Monument, cult.Comp.CurrentTier);

        switch (cult.Comp.CurrentTier)
        {
            case 3:
                _appearance.SetData(ent, MonumentVisuals.Tier3, true);
                break;
            case 2:
                _appearance.SetData(ent, MonumentVisuals.Tier3, false);
                break;
        }

        if (tierUp)
        {
            var transformComp = EnsureComp<MonumentTransformingComponent>(ent);
            transformComp.EndTime = _timing.CurTime + ent.Comp.TransformTime;
            _appearance.SetData(ent, MonumentVisuals.Transforming, true);
        }

        if (finaleComp.CurrentState != FinaleState.Unavailable)
            _appearance.SetData(ent, MonumentVisuals.FinaleReached, true);
    }

    public void UpdateCultData(Entity<MonumentComponent> uid) // This runs every time Entropy is Inserted into The Monument, and every time a Cultist is Converted or Deconverted.
    {
        if (!TryComp<CosmicFinaleComponent>(uid, out var finaleComp))
            return;

        if (AssociatedGamerule(uid) is not {} cult)
            return;

        cult.Comp.TotalCrew = _playerMan.Sessions.Count(session => session.Status == SessionStatus.InGame && HasComp<HumanoidAppearanceComponent>(session.AttachedEntity));

#if DEBUG
        if (cult.Comp.TotalCrew < 25)
            cult.Comp.TotalCrew = 25;
#endif

        cult.Comp.PercentConverted = Math.Round((double)(100 * cult.Comp.TotalCult) / cult.Comp.TotalCrew);

        //this can probably be somewhere else but
        UpdateMonumentReqsForTier(uid, cult.Comp.CurrentTier);

        uid.Comp.CurrentProgress = uid.Comp.TotalEntropy + (cult.Comp.TotalCult * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));

        if (uid.Comp.CurrentProgress >= uid.Comp.TargetProgress && cult.Comp.CurrentTier == 3 && finaleComp.CurrentState == FinaleState.Unavailable)
        {
            if (!finaleComp.FinaleDelayStarted) //check if we've not already started the finale delay
            {
                finaleComp.FinaleDelayStarted = true; //set that we've started it
                //do everything else

                var timer = TimeSpan.FromSeconds(_config.GetCVar(DCCVars.CosmicCultFinaleDelaySeconds));
                var cultistQuery = EntityQueryEnumerator<CosmicCultComponent>();
                while (cultistQuery.MoveNext(out var cultist, out var cultistComp))
                {
                    var mins = timer.Minutes;
                    var secs = timer.Seconds;
                    _antag.SendBriefing(cultist,
                        Loc.GetString("cosmiccult-finale-autocall-briefing",
                            ("minutesandseconds", $"{mins} minutes and {secs} seconds")),
                        Color.FromHex("#4cabb3"),
                        _monumentAlert);
                }

                Timer.Spawn(timer,
                    () =>
                    {
                        ReadyFinale(uid, finaleComp);
                        UpdateCultData(uid); //duplicated work but it looks nicer than calling updateAppearance on it's own
                    });
            }
        }
        else if (finaleComp.CurrentState != FinaleState.Unavailable)
            uid.Comp.TargetProgress = uid.Comp.CurrentProgress;
        else if (uid.Comp.CurrentProgress >= uid.Comp.TargetProgress && cult.Comp.CurrentTier == 2 && uid.Comp.CanTierUp)
        {
            uid.Comp.CanTierUp = false;

            var timer = TimeSpan.FromSeconds(_config.GetCVar(DCCVars.CosmicCultT3RevealDelaySeconds));
            var cultistQuery = EntityQueryEnumerator<CosmicCultComponent>();
            while (cultistQuery.MoveNext(out var cultist, out var cultistComp))
            {
                _antag.SendBriefing(cultist, Loc.GetString("cosmiccult-monument-stage3-briefing", ("time", _config.GetCVar(DCCVars.CosmicCultT3RevealDelaySeconds))), Color.FromHex("#4cabb3"), _monumentAlert);
            }

            MonumentTier3(uid);
            UpdateMonumentReqsForTier(uid, cult.Comp.CurrentTier);

            Timer.Spawn(timer,
                () =>
                {
                    //do spooky things
                    var query = EntityQueryEnumerator<CosmicCultComponent>();
                    while (query.MoveNext(out var cultist, out var cultComp))
                    {
                        EnsureComp<CosmicStarMarkComponent>(cultist);
                    }

                    var sender = Loc.GetString("cosmiccult-announcement-sender");
                    var mapData = _map.GetMap(_transform.GetMapId(cult.Comp.MonumentInGame.Owner.ToCoordinates()));
                    _chatSystem.DispatchStationAnnouncement(uid, Loc.GetString("cosmiccult-announce-tier3-progress"), null, false, null, Color.FromHex("#4cabb3"));
                    _chatSystem.DispatchStationAnnouncement(uid, Loc.GetString("cosmiccult-announce-tier3-warning"), null, false, null, Color.FromHex("#cae8e8"));
                    _audio.PlayGlobal(_tier3Sound, Filter.Broadcast(), false, AudioParams.Default);

                    EnsureComp<ParallaxComponent>(mapData, out var parallax);
                    parallax.Parallax = "CosmicFinaleParallax";
                    Dirty(mapData, parallax);

                    EnsureComp<MapLightComponent>(mapData, out var mapLight);
                    mapLight.AmbientLightColor = Color.FromHex("#210746");
                    Dirty(mapData, mapLight);

                    var lights = EntityQueryEnumerator<PoweredLightComponent>();
                    while (lights.MoveNext(out var light, out _))
                    {
                        if (!_rand.Prob(0.25f))
                            continue;
                        _ghost.DoGhostBooEvent(light);
                    }

                    var collideQuery = EntityQueryEnumerator<MonumentCollisionComponent>();
                    while (collideQuery.MoveNext(out var collideEnt, out var collideComp))
                    {
                        collideComp.HasCollision = true;
                        Dirty(collideEnt, collideComp);
                    }

                    if (TryComp<VisibilityComponent>(uid, out var visComp))
                        _visibility.SetLayer((uid, visComp), 1);

                    cult.Comp.MonumentSlowZone = Spawn("MonumentSlowZone", Transform(uid).Coordinates); // spawn The Monument's slowing fixture entity that supresses non-cult / non-mindshielded / non-chaplain crew.
                    uid.Comp.CanTierUp = true;
                    UpdateCultData(uid); //instantly go up a tier if they manage it.
                    _ui.SetUiState(uid.Owner, MonumentKey.Key, new MonumentBuiState(uid.Comp)); //not sure if this is needed but I'll be safe
                });
        }
        else if (uid.Comp.CurrentProgress >= uid.Comp.TargetProgress && cult.Comp.CurrentTier == 1 && uid.Comp.CanTierUp)
        {
            uid.Comp.CanTierUp = false;

            var cultistQuery = EntityQueryEnumerator<CosmicCultComponent>();
            while (cultistQuery.MoveNext(out var cultist, out var cultistComp))
            {
                _antag.SendBriefing(cultist, Loc.GetString("cosmiccult-monument-stage2-briefing", ("time", _config.GetCVar(DCCVars.CosmicCultT2RevealDelaySeconds))), Color.FromHex("#4cabb3"), _monumentAlert);
            }

            MonumentTier2(uid);
            UpdateMonumentReqsForTier(uid, cult.Comp.CurrentTier);

            Timer.Spawn(TimeSpan.FromSeconds(_config.GetCVar(DCCVars.CosmicCultT2RevealDelaySeconds)),
                () =>
                {
                    //do spooky effects
                    var sender = Loc.GetString("cosmiccult-announcement-sender");
                    var mapData = _map.GetMap(_transform.GetMapId(cult.Comp.MonumentInGame.Owner.ToCoordinates()));
                    _chatSystem.DispatchStationAnnouncement(uid, Loc.GetString("cosmiccult-announce-tier2-progress"), null, false, null, Color.FromHex("#4cabb3"));
                    _chatSystem.DispatchStationAnnouncement(uid, Loc.GetString("cosmiccult-announce-tier2-warning"), null, false, null, Color.FromHex("#cae8e8"));
                    _audio.PlayGlobal(_tier2Sound, Filter.Broadcast(), false, AudioParams.Default);

                    for (var i = 0; i < Convert.ToInt16(cult.Comp.TotalCrew / 4); i++) // spawn # malign rifts equal to 25% of the playercount
                    {
                        if (TryFindRandomTile(out var _, out var _, out var _, out var coords))
                        {
                            Spawn("CosmicMalignRift", coords);
                        }
                    }

                    var lights = EntityQueryEnumerator<PoweredLightComponent>();
                    while (lights.MoveNext(out var light, out _))
                    {
                        if (!_rand.Prob(0.50f))
                            continue;
                        _ghost.DoGhostBooEvent(light);
                    }

                    uid.Comp.CanTierUp = true;
                    UpdateCultData(uid); //instantly go up a tier if they manage it
                    _ui.SetUiState(uid.Owner, MonumentKey.Key, new MonumentBuiState(uid.Comp)); //not sure if this is needed but I'll be safe
                });

        }

        UpdateMonumentAppearance(uid, false);

        Dirty(uid);
        _ui.SetUiState(uid.Owner, MonumentKey.Key, new MonumentBuiState(uid.Comp));
    }

    //note - these ar the thresholds for moving to the next tier
    //so t1 -> 2 needs 20% of the crew
    //t2 -> 3 needs 40%
    //and t3 -> finale needs an extra 20 entropy
    public void UpdateMonumentReqsForTier(Entity<MonumentComponent> monument, int tier)
    {
        if (AssociatedGamerule(monument) is not {} cult)
            return;

        var tier3NumCrew = Math.Round((double)cult.Comp.TotalCrew / 100 * _config.GetCVar(DCCVars.CosmicCultTargetConversionPercent)); // 40% of current pop

        switch (tier)
        {
            case 1:
                monument.Comp.ProgressOffset = 0;
                monument.Comp.TargetProgress = (int)(tier3NumCrew / 2 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));
                break;
            case 2:
                monument.Comp.ProgressOffset = (int)(tier3NumCrew / 2 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue)); //reset the progress offset
                monument.Comp.TargetProgress = (int)(tier3NumCrew * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));
                break;
            case 3:
                monument.Comp.ProgressOffset = (int)(tier3NumCrew * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));
                monument.Comp.TargetProgress = (int)(tier3NumCrew * _config.GetCVar(DCCVars.CosmicCultistEntropyValue)); //removed offset; replaced with timer
                break;
        }
    }

    public void MonumentTier1(Entity<MonumentComponent> uid)
    {
        if (AssociatedGamerule(uid) is not {} cult)
            return;

        cult.Comp.CurrentTier = 1;
        UpdateMonumentAppearance(uid, false);
        cult.Comp.MonumentInGame = uid; //Since there's only one Monument per round, let's store its UID for the rest of the round. Saves us on spamming enumerators.

        //this is probably unnecessary but I have no idea where they get added to the list atm - ruddygreat
        foreach (var glyphProto in _protoMan.EnumeratePrototypes<GlyphPrototype>().Where(proto => proto.Tier == 1))
        {
            uid.Comp.UnlockedGlyphs.Add(glyphProto.ID);
        }

        //basically completely unnecessary, but putting this here for sanity & futureproofing - ruddygreat
        var query = EntityQueryEnumerator<CosmicCultComponent>();
        while (query.MoveNext(out var cultist, out var cultComp))
        {
            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 1))
            {
                cultComp.UnlockedInfluences.Add(influenceProto.ID);
            }

            Dirty(cultist, cultComp);
        }

        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();
        while (objectiveQuery.MoveNext(out _, out var objectiveComp))
        {
            objectiveComp.Tier = 1;
        }
    }

    private void MonumentTier2(Entity<MonumentComponent> uid)
    {
        if (AssociatedGamerule(uid) is not {} cult)
            return;

        cult.Comp.CurrentTier = 2;

        UpdateMonumentAppearance(uid, true);

        foreach (var glyphProto in _protoMan.EnumeratePrototypes<GlyphPrototype>().Where(proto => proto.Tier == 2))
        {
            uid.Comp.UnlockedGlyphs.Add(glyphProto.ID);
        }

        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();
        while (objectiveQuery.MoveNext(out _, out var objectiveComp))
        {
            objectiveComp.Tier = 2;
        }

        var query = EntityQueryEnumerator<CosmicCultComponent>();
        while (query.MoveNext(out var cultist, out var cultComp))
        {
            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 2))
            {
                cultComp.UnlockedInfluences.Add(influenceProto.ID);
            }

            cultComp.EntropyBudget += (int)Math.Floor(Math.Round((double)cult.Comp.TotalCrew / 100 * 10)); // pity system. 10% of the playercount worth of entropy on tier up

            Dirty(cultist, cultComp);
        }

        //add the move action
        var leaderQuery = EntityQueryEnumerator<CosmicCultLeadComponent>();
        while (leaderQuery.MoveNext(out var leader, out var leaderComp))
        {
            _actions.AddAction(leader, ref leaderComp.CosmicMonumentMoveActionEntity, leaderComp.CosmicMonumentMoveAction, leader);
        }

        Dirty(uid);
    }

    private void MonumentTier3(Entity<MonumentComponent> uid)
    {
        if (AssociatedGamerule(uid) is not {} cult)
            return;

        cult.Comp.CurrentTier = 3;

        foreach (var glyphProto in _protoMan.EnumeratePrototypes<GlyphPrototype>().Where(proto => proto.Tier == 3))
        {
            uid.Comp.UnlockedGlyphs.Add(glyphProto.ID);
        }

        UpdateMonumentAppearance(uid, true);

        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();
        while (objectiveQuery.MoveNext(out var _, out var objectiveComp))
        {
            objectiveComp.Tier = 3;
        }

        var query = EntityQueryEnumerator<CosmicCultComponent>();
        while (query.MoveNext(out var cultist, out var cultComp))
        {
            EnsureComp<PressureImmunityComponent>(cultist);
            EnsureComp<TemperatureImmunityComponent>(cultist);

            _damage.SetDamageContainerID(cultist, "BiologicalMetaphysical");

            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 3))
            {
                cultComp.UnlockedInfluences.Add(influenceProto.ID);
            }

            cultComp.Respiration = false;
            cultComp.EntropyBudget += Convert.ToInt16(Math.Floor(Math.Round((double)cult.Comp.TotalCrew / 100 * 10))); //pity system. 10% of the playercount worth of entropy on tier up
            Dirty(cultist, cultComp);
        }

        //remove the move action
        var leaderQuery = EntityQueryEnumerator<CosmicCultLeadComponent>();
        while (leaderQuery.MoveNext(out var leader, out var leaderComp))
        {
            _actions.RemoveAction(leader, leaderComp.CosmicMonumentMoveActionEntity);
        }

        Dirty(uid);
    }

    private void ReadyFinale(Entity<MonumentComponent> uid, CosmicFinaleComponent finaleComp)
    {
        if (TryComp<CosmicCorruptingComponent>(uid, out var comp))
            _corrupting.Enable((uid, comp));

        if (TryComp<ActivatableUIComponent>(uid, out var uiComp))
        {
            if (TryComp<UserInterfaceComponent>(uid, out var uiComp2)) //close the UI for everyone who has it open
            {
                _ui.CloseUi((uid.Owner, uiComp2), MonumentKey.Key);
            }

            uiComp.Key = null; //kazne called this the laziest way to disable a UI ever
        }

        finaleComp.CurrentState = FinaleState.ReadyBuffer;
        uid.Comp.Enabled = false;
        uid.Comp.TargetProgress = uid.Comp.CurrentProgress;

        _popup.PopupCoordinates(Loc.GetString("cosmiccult-finale-ready"), Transform(uid).Coordinates, PopupType.Large);
    }
    #endregion

    #region De- & Conversion
    public void TryStartCult(EntityUid uid, Entity<CosmicCultRuleComponent> rule)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        EnsureComp<CosmicCultComponent>(uid, out var cultComp);
        EnsureComp<IntrinsicRadioReceiverComponent>(uid);
        EnsureComp<CosmicCultAssociatedRuleComponent>(uid, out var associatedComp);

        associatedComp.CultGamerule = rule;

        _role.MindAddRole(mindId, "MindRoleCosmicCult", mind, true);
        _role.MindHasRole<CosmicCultRoleComponent>(mindId, out var cosmicRole);

        if (cosmicRole is not null)
        {
            EnsureComp<RoleBriefingComponent>(cosmicRole.Value.Owner);
            Comp<RoleBriefingComponent>(cosmicRole.Value.Owner).Briefing = Loc.GetString("objective-cosmiccult-charactermenu");
        }

        _antag.SendBriefing(uid, Loc.GetString("cosmiccult-role-roundstart-fluff"), Color.FromHex("#4cabb3"), _briefingSound);
        _antag.SendBriefing(uid, Loc.GetString("cosmiccult-role-short-briefing"), Color.FromHex("#cae8e8"), null);

        var transmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(uid);
        var radio = EnsureComp<ActiveRadioComponent>(uid);
        radio.Channels.Add("CosmicRadio");
        transmitter.Channels.Add("CosmicRadio");

        if (_mind.TryGetSession(mindId, out var session))
        {
            _euiMan.OpenEui(new CosmicRoundStartEui(), session);
        }

        rule.Comp.TotalCult++;

        cultComp.StoredDamageContainer = Comp<DamageableComponent>(uid).DamageContainerID!.Value; // nullable?

        Dirty(uid, cultComp);

        rule.Comp.Cultists.Add(uid);
    }

    private void OnAssociateRule(ref CosmicCultAssociateRuleEvent args)
    {
        TransferCultAssociation(args.Originator, args.Target);
    }

    public void TransferCultAssociation(EntityUid from, EntityUid to)
    {
        if (!TryComp<CosmicCultAssociatedRuleComponent>(from, out var source))
            return;

        var destination = EnsureComp<CosmicCultAssociatedRuleComponent>(to);
        destination.CultGamerule = source.CultGamerule;
    }

    public Entity<CosmicCultRuleComponent>? AssociatedGamerule(EntityUid uid)
    {
        if (!TryComp<CosmicCultAssociatedRuleComponent>(uid, out var associated))
        {
            _sawmill.Debug("{0} has no associated rule", uid);
            return null;
        }

        if (!TryComp<CosmicCultRuleComponent>(associated.CultGamerule, out var cult))
        {
            _sawmill.Debug("Associated gamerule {0} is not a cult gamerule", associated.CultGamerule);
            return null;
        }

        return (associated.CultGamerule, cult);
    }

    public void CosmicConversion(EntityUid converter, EntityUid uid)
    {
        if (AssociatedGamerule(converter) is not {} cult)
            return;
        var cosmicGamerule = cult.Comp;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        _role.MindAddRole(mindId, "MindRoleCosmicCult", mind, true);
        _role.MindHasRole<CosmicCultRoleComponent>(mindId, out var cosmicRole);

        if (cosmicRole is not null)
        {
            EnsureComp<RoleBriefingComponent>(cosmicRole.Value.Owner);
            Comp<RoleBriefingComponent>(cosmicRole.Value.Owner).Briefing = Loc.GetString("objective-cosmiccult-charactermenu");
        }

        _antag.SendBriefing(mind.Session, Loc.GetString("cosmiccult-role-conversion-fluff"), Color.FromHex("#4cabb3"), _briefingSound);
        _antag.SendBriefing(uid, Loc.GetString("cosmiccult-role-short-briefing"), Color.FromHex("#cae8e8"), null);

        var cultComp = EnsureComp<CosmicCultComponent>(uid);
        cultComp.EntropyBudget = 10; // pity balance
        cultComp.StoredDamageContainer = Comp<DamageableComponent>(uid).DamageContainerID!.Value;
        EnsureComp<IntrinsicRadioReceiverComponent>(uid);
        TransferCultAssociation(converter, uid);

        if (cosmicGamerule.CurrentTier == 3)
        {
            _damage.SetDamageContainerID(uid, "BiologicalMetaphysical");
            cultComp.EntropyBudget = 48; // pity balance
            cultComp.Respiration = false;

            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 3))
            {
                cultComp.UnlockedInfluences.Add(influenceProto.ID);
            }

            EnsureComp<CosmicStarMarkComponent>(uid);
            EnsureComp<PressureImmunityComponent>(uid);
            EnsureComp<TemperatureImmunityComponent>(uid);
        }
        else if (cosmicGamerule.CurrentTier == 2)
        {
            cultComp.EntropyBudget = 26; // pity balance

            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 2))
            {
                cultComp.UnlockedInfluences.Add(influenceProto.ID);
            }
        }

        Dirty(uid, cultComp);

        var transmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(uid);
        var radio = EnsureComp<ActiveRadioComponent>(uid);
        radio.Channels = new() { "CosmicRadio" };
        transmitter.Channels = new() { "CosmicRadio" };

        _mind.TryAddObjective(mindId, mind, "CosmicFinalityObjective");
        _mind.TryAddObjective(mindId, mind, "CosmicMonumentObjective");
        _mind.TryAddObjective(mindId, mind, "CosmicEntropyObjective");

        if (_mind.TryGetSession(mindId, out var session))
        {
            _euiMan.OpenEui(new CosmicConvertedEui(), session);
        }

        RemComp<BibleUserComponent>(uid);

        cosmicGamerule.TotalCult++;
        cosmicGamerule.Cultists.Add(uid);

        UpdateCultData(cosmicGamerule.MonumentInGame);
    }
    private void OnComponentShutdown(Entity<CosmicCultComponent> uid, ref ComponentShutdown args)
    {
        if (AssociatedGamerule(uid) is not {} cult)
            return;
        var cosmicGamerule = cult.Comp;

        _stun.TryKnockdown(uid, TimeSpan.FromSeconds(2), true);
        foreach (var actionEnt in uid.Comp.ActionEntities) _actions.RemoveAction(actionEnt);

        if (TryComp<IntrinsicRadioTransmitterComponent>(uid, out var transmitter))
            transmitter.Channels.Remove("CosmicRadio");
        if (TryComp<ActiveRadioComponent>(uid, out var radio))
            radio.Channels.Remove("CosmicRadio");
        RemComp<CosmicCultLeadComponent>(uid);
        RemComp<InfluenceVitalityComponent>(uid);
        RemComp<InfluenceStrideComponent>(uid);
        RemComp<PressureImmunityComponent>(uid);
        RemComp<TemperatureImmunityComponent>(uid);
        RemComp<CosmicStarMarkComponent>(uid);
        _damage.SetDamageContainerID(uid, uid.Comp.StoredDamageContainer);
        _antag.SendBriefing(uid, Loc.GetString("cosmiccult-role-deconverted-fluff"), Color.FromHex("#4cabb3"), _deconvertSound);
        _antag.SendBriefing(uid, Loc.GetString("cosmiccult-role-deconverted-briefing"), Color.FromHex("#cae8e8"), null);

        if (!_mind.TryGetMind(uid, out var mindId, out _) || !TryComp<MindComponent>(mindId, out var mindComp))
            return;

        _mind.ClearObjectives(mindId, mindComp); // LOAD-BEARING #imp function to remove all of someone's objectives, courtesy of TCRGDev(Github)
        _role.MindTryRemoveRole<CosmicCultRoleComponent>(mindId);
        _role.MindTryRemoveRole<RoleBriefingComponent>(mindId);
        if (_mind.TryGetSession(mindId, out var session))
        {
            _euiMan.OpenEui(new CosmicDeconvertedEui(), session);
        }
        _eye.SetVisibilityMask(uid, 1);
        _alerts.ClearAlert(uid, uid.Comp.EntropyAlert);
        cosmicGamerule.TotalCult--;
        cosmicGamerule.Cultists.Remove(uid);
        UpdateCultData(cosmicGamerule.MonumentInGame);
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }
    #endregion
}
