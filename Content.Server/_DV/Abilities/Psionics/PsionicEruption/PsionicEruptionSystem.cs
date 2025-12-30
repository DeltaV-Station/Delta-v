using Content.Server._DV.Psionics;
using Content.Server.Abilities.Psionics;
using Content.Server.DoAfter;
using Content.Server.EUI;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Jittering;
using Content.Server.Lightning;
using Content.Server.Mind;
using Content.Shared._DV.Abilities.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.Psionics.Glimmer;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Abilities.Psionics;

public sealed class PsionicEruptionSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly JitteringSystem _jittering = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly EntProtoId? Sparks = "EffectSparks";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicEruptionPowerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PsionicEruptionPowerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PsionicEruptionPowerComponent, PsionicEruptionPowerActionEvent>(OnPowerUsed);

        SubscribeLocalEvent<PsionicEruptionPowerComponent, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<PsionicEruptionPowerComponent, PsionicEruptionDoAfterEvent>(OnDoAfter);
    }

    private void OnInit(Entity<PsionicEruptionPowerComponent> entity, ref ComponentInit args)
    {
        var component = entity.Comp;
        _actions.AddAction(entity, ref component.EruptionActionEntity, component.EruptionActionId);

        if (_actions.GetAction(component.EruptionActionEntity) is { Comp.UseDelay: not null } action)
        {
            _actions.StartUseDelay(action.Owner);
        }

        if (TryComp<PsionicComponent>(entity, out var psionic) && psionic.PsionicAbility == null)
        {
            psionic.PsionicAbility = component.EruptionActionEntity;
            psionic.ActivePowers.Add(component);
        }
    }

    private void ShowWarning(Entity<PsionicEruptionPowerComponent> entity)
    {
        var comp = entity.Comp;
        if (comp.Warned)
            return;
        MindComponent? mind;
        if (!_mind.TryGetMind(entity, out var _, out mind))
            return;
        if (mind.UserId == null || !_player.TryGetSessionById(mind.UserId.Value, out var client))
            return;
        _eui.OpenEui(new EruptionWarningEui(), client);
        comp.Warned = true;
    }

    private void OnShutdown(Entity<PsionicEruptionPowerComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.EruptionActionEntity);

        if (TryComp<PsionicComponent>(entity, out var psionic))
        {
            psionic.ActivePowers.Remove(entity.Comp);
        }
    }

    private void OnDispelled(Entity<PsionicEruptionPowerComponent> entity, ref DispelledEvent args)
    {
        if (entity.Comp.DoAfter == null)
            return;

        _doAfter.Cancel(entity.Comp.DoAfter);
        entity.Comp.DoAfter = null;

        args.Handled = true;
    }

    private void OnPowerUsed(Entity<PsionicEruptionPowerComponent> entity, ref PsionicEruptionPowerActionEvent args)
    {
        var component = entity.Comp;
        int detonateTime = 10;
        int sparkFrom = 5;

        if (_glimmer.GetGlimmerTier(_glimmer.Glimmer) == GlimmerTier.Critical)
        {
            detonateTime = 30;
            sparkFrom = 15;
        }

        var ev = new PsionicEruptionDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, entity, detonateTime, ev, entity);
        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        component.DoAfter = doAfterId;
        _popups.PopupEntity(Loc.GetString("psionic-eruption-begin", ("user", entity)), entity, PopupType.LargeCaution); // Loc.GetString("psionic-regeneration-begin")
        _audio.PlayPvs(component.SoundUse, entity, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
        _psionics.LogPowerUsed(entity, "psionic eruption", 2, 4);
        // Start Jittering
        _jittering.DoJitter(entity, TimeSpan.FromSeconds(sparkFrom), true, 10, 16);

        component.NextSpark = _gameTiming.CurTime + TimeSpan.FromSeconds(sparkFrom);
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Occasionally pester users of the Psionic Eruption power to use it.
        var t = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<PsionicEruptionPowerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // All of the timespan data should really get moved out of the system
            ShowWarning((uid, comp)); // I'm a bad coder.
            if (comp.DoAfter != null)
            {
                if (t > comp.NextSpark)
                {
                    if (_glimmer.GetGlimmerTier(_glimmer.Glimmer) == GlimmerTier.Critical && _random.Prob(0.125f))
                    {
                        _lightning.ShootRandomLightnings(uid, 5f, _random.Next(1, 3));
                    }
                    _jittering.DoJitter(uid, TimeSpan.FromSeconds(5), true, 10, 32);
                    Spawn(Sparks, Transform(uid).Coordinates);

                    comp.NextSpark = t + TimeSpan.FromMilliseconds(500);
                }
                continue;
            }
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
                    Spawn(Sparks, Transform(uid).Coordinates);
                    break;
                case GlimmerTier.Dangerous:
                    msg = "psionic-eruption-annoy-dangerous";
                    minwait = TimeSpan.FromSeconds(20);
                    msgSize = PopupType.Large;
                    Spawn(Sparks, Transform(uid).Coordinates);
                    break;
                case GlimmerTier.Critical:
                    msg = "psionic-eruption-annoy-critical";
                    minwait = TimeSpan.FromSeconds(10);
                    msgSize = PopupType.LargeCaution;
                    Spawn(Sparks, Transform(uid).Coordinates);
                    break;
            }
            // Prompt the user to use the power.
            _popups.PopupEntity(Loc.GetString(msg, ("user", uid)), uid, uid, msgSize);
            comp.NextAnnoy = t + minwait + TimeSpan.FromSeconds(_random.Next(0, 10)); // Add a random delay to the next annoyance.
        }
    }
    private void OnDoAfter(Entity<PsionicEruptionPowerComponent> entity, ref PsionicEruptionDoAfterEvent args)
    {
        entity.Comp.DoAfter = null;

        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp<BodyComponent>(entity, out var body))
            return;

        var pos = _transform.GetMapCoordinates(entity);
        // acidify: false preserves inventory items when erupting
        _body.GibBody(entity, acidify: false, body, launchGibs: true);
        int boom = _glimmer.GetGlimmerTier(_glimmer.Glimmer) switch
        {
            GlimmerTier.Minimal => 4,
            GlimmerTier.Low => 8,
            GlimmerTier.Moderate => 12,
            GlimmerTier.High => 16,
            GlimmerTier.Dangerous => 32,
            GlimmerTier.Critical => 64,
            _ => 0
        };
        _explosion.QueueExplosion(pos, ExplosionSystem.DefaultExplosionPrototypeId, boom, 1, 5, entity, maxTileBreak: 0);
    }
}
