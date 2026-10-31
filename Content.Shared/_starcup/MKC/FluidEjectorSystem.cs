using System.Linq;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage.Systems;
using Content.Shared.Fluids;
using Content.Shared.Forensics.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._starcup.MKC;

public sealed class FluidEjectorSystem : EntitySystem
{
    public static readonly EntProtoId Drunk = "StatusEffectDrunk";

    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, SolutionContainerChangedEvent>(_body.RelayEvent);
        SubscribeLocalEvent<FluidEjectorComponent, BodyRelayedEvent<SolutionContainerChangedEvent>>(OnSolutionChanged);
    }

    public override void Update(float deltaTime)
    {
        var query = EntityQueryEnumerator<FluidEjectorComponent, OrganComponent>();
        while (query.MoveNext(out _, out var fluidEjector, out var organ))
        {
            if (fluidEjector.NextUpdate == TimeSpan.Zero)
                continue;

            if (organ.Body is not {} body)
                continue;

            if (_gameTiming.CurTime >= fluidEjector.NextPopupTime)
            {
                fluidEjector.NextPopupTime = _gameTiming.CurTime + fluidEjector.PopupCooldown;
                _popup.PopupClient(Loc.GetString("fluid-regulator-warning"), body, body, PopupType.LargeCaution);
            }

            if (_gameTiming.CurTime >= fluidEjector.NextUpdate)
            {
                fluidEjector.NextUpdate = TimeSpan.Zero;
                fluidEjector.NextPopupTime = TimeSpan.Zero;
                DoFluidEject(body, fluidEjector);
            }
        }
    }

    private void OnSolutionChanged(Entity<FluidEjectorComponent> ent, ref BodyRelayedEvent<SolutionContainerChangedEvent> args)
    {
        if (_mobState.IsDead(args.Body.Owner))
            return;

        if (args.Args.SolutionId != ent.Comp.Solution)
            return;

        var bloodReagentEvent = new MetabolismExclusionEvent();
        RaiseLocalEvent(args.Body.Owner, ref bloodReagentEvent);

        var metabolismWhitelistEvent = new MetabolismWhitelistEvent();
        RaiseLocalEvent(args.Body.Owner, ref metabolismWhitelistEvent);

        var bodyReagents = args.Args.Solution.Contents.Select(r => r.Reagent.Prototype);
        var bloodReferenceReagents = bloodReagentEvent.Reagents.Select(reagentId => reagentId.Prototype);
        var whitelistedReagents = metabolismWhitelistEvent.Reagents.Select(protoId => protoId.Id);
        if (!bodyReagents.Any(id => !bloodReferenceReagents.Contains(id) && !whitelistedReagents.Contains(id)))
            return;

        if (ent.Comp.NextUpdate != TimeSpan.Zero)
            return;

        ent.Comp.NextUpdate = _gameTiming.CurTime + ent.Comp.EjectionTime;
    }

    private Solution? GetEjectedReagents(EntityUid uid)
    {
        BloodstreamComponent? bloodstream = null;
        if (!Resolve(uid, ref bloodstream))
            return null;

        if (!_solutionContainer.ResolveSolution(uid,
                bloodstream.BloodSolutionName,
                ref bloodstream.BloodSolution,
                out var bloodSolution))
        {
            Log.Error("Failed to resolve bloodstream solution for entity {}", uid);
            return null;
        }

        // collect reagents which should not be metabolized in the blood stream (because they are the entity's blood)
        var ev = new MetabolismExclusionEvent();
        RaiseLocalEvent(uid, ref ev);

        var bloodReferenceReagents = ev.Reagents.Select(reagentId => new ProtoId<ReagentPrototype>(reagentId.Prototype)).ToArray();
        var ejectedSolution = _solutionContainer.SplitSolutionWithout(bloodstream.BloodSolution.Value,
            bloodSolution.Volume,
            bloodReferenceReagents);

        return ejectedSolution;
    }

    private void DoFluidEject(EntityUid uid, FluidEjectorComponent fluidEjector)
    {
        var ejectedSolution = GetEjectedReagents(uid);
        if (ejectedSolution == null)
            return;

        var ejectedAmount = ejectedSolution.Volume;

        if (_puddle.TrySpillAt(uid, ejectedSolution, out var puddle))
            _forensics.TransferDna(puddle, uid, false);

        var slowdownTime = TimeSpan.FromSeconds(Math.Clamp((ejectedAmount * 0.2f).Value, 0, 600)); // clamped at 10 minutes to prevert forever-slows
        _movementMod.TryUpdateMovementSpeedModDuration(uid,
            MovementModStatusSystem.VomitingSlowdown,
            slowdownTime,
            0.5f);

        var drunkennessTime = slowdownTime * 1.5;
        _status.TryUpdateStatusEffectDuration(uid, Drunk, drunkennessTime);

        _popup.PopupPredicted(Loc.GetString("fluid-regulator-eject", ("person", Identity.Entity(uid, EntityManager))), uid, uid, PopupType.Large);

        var damage = ejectedAmount * fluidEjector.EjectionDamage * fluidEjector.EjectionDamageMultiplier;
        _damageableSystem.TryChangeDamage(uid, damage, ignoreResistances: true);
    }
}
