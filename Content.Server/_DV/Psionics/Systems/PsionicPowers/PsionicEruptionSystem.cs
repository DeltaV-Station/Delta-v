using Content.Server.Abilities.Psionics;
using Content.Server.DoAfter;
using Content.Server.EUI;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Jittering;
using Content.Server.Lightning;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class PsionicEruptionSystem : BasePsionicPowerSystem<PsionicEruptionPowerComponent, PsionicEruptionPowerActionEvent>
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly JitteringSystem _jittering = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly EntProtoId? Sparks = "EffectSparks";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicEruptionPowerComponent, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<PsionicEruptionPowerComponent, PsionicEruptionDoAfterEvent>(OnDoAfter);
    }

    protected override void OnPowerInit(Entity<PsionicEruptionPowerComponent> power, ref MapInitEvent args)
    {
        base.OnPowerInit(power, ref args);

        if (!_player.TryGetSessionByEntity(power, out var session))
            return;

        _eui.OpenEui(new EruptionWarningEui(), session);
    }

    private void OnDispelled(Entity<PsionicEruptionPowerComponent> entity, ref DispelledEvent args)
    {
        if (entity.Comp.DoAfterId == null)
            return;

        _doAfter.Cancel(entity.Comp.DoAfterId);
        entity.Comp.DoAfterId = null;

        args.Handled = true;
    }

    protected override void OnPowerUsed(Entity<PsionicEruptionPowerComponent> psionic, ref PsionicEruptionPowerActionEvent args)
    {
        TimeSpan detonateTime;
        TimeSpan sparkFrom;

        if (_glimmer.GetGlimmerTier(_glimmer.Glimmer) == GlimmerTier.Critical)
        {
            detonateTime = psionic.Comp.MaxDetonateDelay;
            sparkFrom = detonateTime / 2;
        }
        else
        {
            detonateTime = psionic.Comp.MinDetonateDelay;
            sparkFrom = detonateTime / 2;
        }

        // Start the DoAfter.
        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, detonateTime, new PsionicEruptionDoAfterEvent(), args.Performer);
        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        psionic.Comp.DoAfterId = doAfterId; // Save the DoAfterID to reference it later.

        var message = Loc.GetString("psionic-eruption-begin", ("user", psionic));
        _popups.PopupEntity(message, args.Performer, PopupType.LargeCaution);
        _audio.PlayPvs(psionic.Comp.SoundUse, args.Performer, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
        _jittering.DoJitter(args.Performer, sparkFrom, true, 10, 16);

        LogPowerUsed(psionic, args.Performer, psionic.Comp.PowerName, psionic.Comp.MinGlimmerChanged, psionic.Comp.MaxGlimmerChanged);

        psionic.Comp.NextSpark = _gameTiming.CurTime + sparkFrom;
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Occasionally pester users of the Psionic Eruption power to use it.
        var curTime = _gameTiming.CurTime;

        var query = EntityQueryEnumerator<PsionicEruptionPowerComponent>();
        while (query.MoveNext(out var psionic, out var comp))
        {
            if (comp.DoAfterId != null)
            {
                if (curTime < comp.NextSpark)
                    continue;
                CauseSparks(psionic, comp, curTime);
            }

            if (curTime < comp.NextAnnoy)
                continue;

            _glimmer.Glimmer += _random.Next(1, 5); // Increase glimmer by a random amount.

            var msg = GetSeverityMessage(psionic, out var messageSize, out var minWait);
            // Prompt the user to use the power.
            _popups.PopupEntity(Loc.GetString(msg, ("user", psionic)), psionic, psionic, messageSize);
            comp.NextAnnoy = curTime + minWait + TimeSpan.FromSeconds(_random.Next(0, 10)); // Add a random delay to the next annoyance.
        }
    }

    private string GetSeverityMessage(EntityUid psionic, out PopupType messageSize, out TimeSpan minWait)
    {
        string message;
        switch (_glimmer.GetGlimmerTier(_glimmer.Glimmer))
        {
            case GlimmerTier.Minimal:
            default:
                message = "psionic-eruption-annoy-minimal";
                minWait = TimeSpan.FromSeconds(60);
                messageSize = PopupType.Small;
                break;
            case GlimmerTier.Low:
                message = "psionic-eruption-annoy-low";
                minWait = TimeSpan.FromSeconds(45);
                messageSize = PopupType.Small;
                break;
            case GlimmerTier.Moderate:
                message = "psionic-eruption-annoy-moderate";
                minWait = TimeSpan.FromSeconds(30);
                messageSize = PopupType.Small;
                break;
            case GlimmerTier.High:
                message = "psionic-eruption-annoy-high";
                minWait = TimeSpan.FromSeconds(25);
                messageSize = PopupType.Medium;
                Spawn(Sparks, Transform(psionic).Coordinates);
                break;
            case GlimmerTier.Dangerous:
                message = "psionic-eruption-annoy-dangerous";
                minWait = TimeSpan.FromSeconds(20);
                messageSize = PopupType.Large;
                Spawn(Sparks, Transform(psionic).Coordinates);
                break;
            case GlimmerTier.Critical:
                message = "psionic-eruption-annoy-critical";
                minWait = TimeSpan.FromSeconds(10);
                messageSize = PopupType.LargeCaution;
                Spawn(Sparks, Transform(psionic).Coordinates);
                break;
        }

        return message;
    }

    private void CauseSparks(EntityUid psionic, PsionicEruptionPowerComponent comp, TimeSpan curTime)
    {
        if (_glimmer.GetGlimmerTier(_glimmer.Glimmer) == GlimmerTier.Critical && _random.Prob(0.125f))
        {
            _lightning.ShootRandomLightnings(psionic, 5f, _random.Next(1, 3));
        }
        _jittering.DoJitter(psionic, TimeSpan.FromSeconds(5), true, 10, 32);
        Spawn(Sparks, Transform(psionic).Coordinates);

        comp.NextSpark = curTime + TimeSpan.FromMilliseconds(500);
    }

    private void OnDoAfter(Entity<PsionicEruptionPowerComponent> psionic, ref PsionicEruptionDoAfterEvent args)
    {
        psionic.Comp.DoAfterId = null;

        if (args.Cancelled
            || args.Handled
            || !TryComp<BodyComponent>(args.User, out var body))
            return;

        var pos = _transform.GetMapCoordinates(args.User);
        _body.GibBody(args.User, acidify: true, body, launchGibs: true);

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
        _explosion.QueueExplosion(pos, ExplosionSystem.DefaultExplosionPrototypeId, boom, 1, 5, psionic, maxTileBreak: 0);
    }
}
