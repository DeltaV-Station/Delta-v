using System.Threading;
using Robust.Shared.Audio;
using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Actions.Events;
using Content.Shared.Body.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.DoAfter;
using Content.Server.DoAfter;
using Content.Shared.Psionics.Events;
using Robust.Shared.Timing;
using Robust.Server.Audio;
using Timer = Robust.Shared.Timing.Timer;
using Content.Server.Jittering;
using Content.Server.Lightning;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Robust.Shared.Player;
using Content.Server.RoundEnd;
using Robust.Shared.Utility;
using Content.Shared.Humanoid;
using Content.Server.EUI;
using Content.Server.Psionics;
using Content.Shared.Mind;
using Robust.Server.Player;

using Content.Server.Mind;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicEruptionSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedBodySystem _body = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly JitteringSystem _jitteringSystem = default!;
        [Dependency] private readonly LightningSystem _lightning = default!;
        [Dependency] private readonly GlimmerSystem _glimmer = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly NavMapSystem _navMap = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly RoundEndSystem _roundEnd = default!;
        [Dependency] private readonly EuiManager _euiManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;

        private readonly SoundSpecifier _nukeAlertSound = new SoundPathSpecifier("/Audio/Misc/redalert.ogg");

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicEruptionPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicEruptionPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicEruptionPowerComponent, PsionicEruptionPowerActionEvent>(OnPowerUsed);

            SubscribeLocalEvent<PsionicEruptionPowerComponent, DispelledEvent>(OnDispelled);
            SubscribeLocalEvent<PsionicEruptionPowerComponent, PsionicEruptionDoAfterEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, PsionicEruptionPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.EruptionActionEntity, component.EruptionActionId);
            _actions.TryGetActionData(component.EruptionActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.EruptionActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            {
                psionic.PsionicAbility = component.EruptionActionEntity;
                psionic.ActivePowers.Add(component);
            }
        }

        private void ShowWarning(EntityUid uid, PsionicEruptionPowerComponent comp)
        {
            if (comp.Warned)
                return;
            MindComponent? mind = null;
            if (!_mindSystem.TryGetMind(uid, out var _, out mind))
                return;
            if (mind.UserId == null || !_playerManager.TryGetSessionById(mind.UserId.Value, out var client))
                return;
            _euiManager.OpenEui(new EruptionWarningEui(uid, this), client);
            comp.Warned = true;
        }

        private void OnShutdown(EntityUid uid, PsionicEruptionPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.EruptionActionEntity);

            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
            }
        }

        private void OnDispelled(EntityUid uid, PsionicEruptionPowerComponent component, DispelledEvent args)
        {
            if (component.DoAfter == null)
                return;

            _doAfterSystem.Cancel(component.DoAfter);
            component.DoAfter = null;

            args.Handled = true;
        }

        private void OnPowerUsed(EntityUid uid, PsionicEruptionPowerComponent component, PsionicEruptionPowerActionEvent args)
        {
            int detonateTime = 10;
            int sparkFrom = 5;

            // This is going to nuke the station,
            if (_glimmer.GetGlimmerTier(_glimmer.Glimmer) == GlimmerTier.Critical)
            {
                // Count living creatures nearby
                var mindsCount = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, 5f).Count;
                if (mindsCount < 3)
                {
                    _popups.PopupEntity(Loc.GetString("psionic-eruption-not-enough-creatures", ("count", $"{mindsCount}")), uid, uid, PopupType.Medium);
                    return;
                }

                detonateTime = 30;
                sparkFrom = 15;
                var pos = Transform(uid);
                var stationUid = _station.GetStationInMap(pos.MapID);
                var announcement = Loc.GetString("psionic-eruption-nuke-warning", ("location", FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((uid, pos)))));
                var sender = Loc.GetString("psionic-eruption-nuke-sender");
                _chatSystem.DispatchStationAnnouncement(stationUid ?? uid, announcement, sender, false, null, Color.Red);
                _audioSystem.PlayGlobal(_nukeAlertSound, Filter.Broadcast(), false, AudioParams.Default);
            }

            var ev = new PsionicEruptionDoAfterEvent(_gameTiming.CurTime);
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, detonateTime, ev, uid);
            _doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId);
            component.DoAfter = doAfterId;
            _popups.PopupEntity(Loc.GetString("psionic-eruption-begin", ("user", uid)), uid, PopupType.LargeCaution); // Loc.GetString("psionic-regeneration-begin")
            _audioSystem.PlayPvs(component.SoundUse, component.Owner, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
            _psionics.LogPowerUsed(uid, "psionic eruption", 2, 4);
            // Start Jittering
            _jitteringSystem.DoJitter(uid, TimeSpan.FromSeconds(sparkFrom), true, 10, 16);
            for (int i = sparkFrom * 1000; i < detonateTime * 1000; i += 500)
                Timer.Spawn(i,
                    () =>
                    {
                        // Ensure the DoAfter is still valid before proceeding.
                        if (component.DoAfter == null)
                            return;
                        if (_glimmer.GetGlimmerTier(_glimmer.Glimmer) == GlimmerTier.Critical && _random.Prob(0.125f))
                        {
                            _lightning.ShootRandomLightnings(uid, 5f, _random.Next(1, 3), "Lightning", 0, false);
                        }
                        _jitteringSystem.DoJitter(uid, TimeSpan.FromSeconds(5), true, 10, 32);
                        Spawn("EffectSparks", Transform(uid).Coordinates);
                    }, // it gibs, damage doesn't need to be high.
                    CancellationToken.None);

            args.Handled = true;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            // Occasionally pester users of the Psionic Eruption power to use it.
            var t = _gameTiming.CurTime;
            foreach (var comp in EntityQuery<PsionicEruptionPowerComponent>())
            {
                var uid = comp.Owner;
                ShowWarning(uid, comp); // I'm a bad coder.
                if (comp.DoAfter != null)
                    continue;
                if (t < comp.NextAnnoy)
                    continue;
                var msg = "psionic-eruption-annoy-minimal";
                var msgSize = PopupType.Small;
                var minwait = TimeSpan.FromSeconds(60); // How many seconds to wait before the next annoyance.
                _glimmer.Glimmer += _random.Next(1, 5); // Increase glimmer by a random amount.
                switch (_glimmer.GetGlimmerTier(_glimmer.Glimmer))
                {
                    case GlimmerTier.Minimal:
                        msg = "psionic-eruption-annoy-minimal";
                        minwait = TimeSpan.FromSeconds(60);
                        break;
                    case GlimmerTier.Low:
                        msg = "psionic-eruption-annoy-low";
                        minwait = TimeSpan.FromSeconds(45);
                        break;
                    case GlimmerTier.Moderate:
                        msg = "psionic-eruption-annoy-moderate";
                        minwait = TimeSpan.FromSeconds(30);
                        break;
                    case GlimmerTier.High:
                        msg = "psionic-eruption-annoy-high";
                        minwait = TimeSpan.FromSeconds(25);
                        msgSize = PopupType.Medium;
                        Spawn("EffectSparks", Transform(uid).Coordinates);
                        break;
                    case GlimmerTier.Dangerous:
                        msg = "psionic-eruption-annoy-dangerous";
                        minwait = TimeSpan.FromSeconds(20);
                        msgSize = PopupType.Large;
                        Spawn("EffectSparks", Transform(uid).Coordinates);
                        break;
                    case GlimmerTier.Critical:
                        msg = "psionic-eruption-annoy-critical";
                        minwait = TimeSpan.FromSeconds(10);
                        msgSize = PopupType.LargeCaution;
                        Spawn("EffectSparks", Transform(uid).Coordinates);
                        break;
                }
                // Prompt the user to use the power.
                _popups.PopupEntity(Loc.GetString(msg, ("user", uid)), uid, uid, msgSize);
                comp.NextAnnoy = t + minwait + TimeSpan.FromSeconds(_random.Next(0, 10)); // Add a random delay to the next annoyance.
            }
        }
        private void OnDoAfter(EntityUid uid, PsionicEruptionPowerComponent component, PsionicEruptionDoAfterEvent args)
        {
            component.DoAfter = null;

            if (args.Cancelled || args.Handled)
                return;

            if (!TryComp<BodyComponent>(uid, out var body))
                return;

            var pos = _transformSystem.GetMapCoordinates(uid);
            _body.GibBody(uid, acidify: true, body, launchGibs: true);
            int boom = 4;
            switch (_glimmer.GetGlimmerTier(_glimmer.Glimmer))
            {
                case GlimmerTier.Minimal:
                    boom = 4;
                    break;
                case GlimmerTier.Low:
                    boom = 8;
                    break;
                case GlimmerTier.Moderate:
                    boom = 12;
                    break;
                case GlimmerTier.High:
                    boom = 16;
                    break;
                case GlimmerTier.Dangerous:
                    boom = 32;
                    break;
                case GlimmerTier.Critical:
                    _explosionSystem.QueueExplosion(pos, ExplosionSystem.DefaultExplosionPrototypeId, 5000000, 5, 100, uid);
                    Timer.Spawn(10000,
                    () =>
                    {
                        _roundEnd.EndRound();
                    });
                    return;
            }
            _explosionSystem.QueueExplosion(pos, ExplosionSystem.DefaultExplosionPrototypeId, boom, 1, 2, uid, maxTileBreak: 0);
        }
    }
}
