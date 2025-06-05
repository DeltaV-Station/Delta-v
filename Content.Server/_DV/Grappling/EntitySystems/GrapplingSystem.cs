using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Interaction;
using Content.Server.Popups;
using Content.Shared._DV.Grappling.Components;
using Content.Shared._DV.Grappling.EntitySystems;
using Content.Shared._DV.Grappling.Events;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Grappling.EntitySystems;

/// <summary>
/// Service side handling of grappling
/// </summary>
public sealed partial class GrapplingSystem : SharedGrapplingSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly InteractionSystem _interactionSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StandingStateSystem _standingStateSystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private ProtoId<TagPrototype> _grappleTargetId = "GrappleTarget";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrapplerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<GrapplerComponent, PullStoppedMessage>(OnPullStopped);
        SubscribeLocalEvent<GrapplerComponent, EscapeGrappleAlertEvent>(OnEscapeGrapplerAlert);

        SubscribeLocalEvent<GrappledComponent, ComponentShutdown>(OnGrappledShutdown);
        SubscribeLocalEvent<GrappledComponent, MoveInputEvent>(OnGrappledMove);
        SubscribeLocalEvent<GrappledComponent, GrappledEscapeDoAfter>(OnEscapeDoAfter);
        SubscribeLocalEvent<GrappledComponent, EscapeGrappleAlertEvent>(OnEscapeGrappledAlert);
    }

    /// <summary>
    /// Validates whether a grappler can actually grapple the victim.
    /// </summary>
    /// <param name="grappler">Entity performing the grapple.</param>
    /// <param name="victim">Intended victim of the grapple.</param>
    /// <returns>True if the grapple could start, false otherwise.</returns>
    public bool CanGrapple(Entity<GrapplerComponent?> grappler, EntityUid victim)
    {
        if (!Resolve(grappler, ref grappler.Comp))
            return false;

        if (grappler.Comp.ActiveVictim.HasValue)
            return false; // Can't grapple a second person

        if (_gameTiming.CurTime < grappler.Comp.CooldownEnd)
            return false; // Cooldown on the grapple is not over yet

        if (!_actionBlockerSystem.CanInteract(grappler, victim))
            return false;

        if (!TryComp<TagComponent>(victim, out var tagComp) ||
            !_tagSystem.HasTag(tagComp, _grappleTargetId))
            return false; // Not a valid target

        return true;
    }

    /// <summary>
    /// Returns whether a grapple is currently active on a victim.
    /// </summary>
    /// <param name="grappler">The grappler to check.</param>
    /// <returns>True if there is an ongoing grapple, false otherwise.</returns>
    public bool IsGrappling(Entity<GrapplerComponent?> grappler)
    {
        if (!Resolve(grappler, ref grappler.Comp))
            return false; // Isn't a grappler, so nothing to do.

        return grappler.Comp.ActiveVictim.HasValue;
    }

    /// <summary>
    /// Attempts to start grappling a victim, rendering them unable to move and possibly
    /// removing their ability to use their hands.
    /// </summary>
    /// <param name="grappler">Entity attempting to begin a grapple</param>
    /// <param name="victim">Entity to be grappled.</param>
    /// <returns>True if the grapple started, false otherwise.</returns>
    public bool TryStartGrapple(Entity<GrapplerComponent?> grappler, EntityUid victim)
    {
        if (!Resolve(grappler, ref grappler.Comp))
            return false;

        if (!CanGrapple(grappler, victim))
            return false;

        if (!_interactionSystem.InRangeUnobstructed(grappler.Owner, victim))
            return false;

        StartGrapple((grappler, grappler.Comp), victim, startPulling: true);
        return true;
    }

    /// <summary>
    /// Releases a victim from a grapple allowing them to move again and returning any
    /// hands that were disabled.
    /// </summary>
    /// <param name="grappler">Entity, which was performing the grapple.</param>
    /// <param name="manualRelease">Whether this release is considered by the grappler, or by the grappled.</param>
    /// <returns>True if the grapple was released, false otherwise.</returns>
    public bool ReleaseGrapple(Entity<GrapplerComponent?> grappler, bool manualRelease = false)
    {
        if (!Resolve(grappler, ref grappler.Comp))
            return false;

        var victim = grappler.Comp.ActiveVictim;
        if (!victim.HasValue)
            return false; // Not grappling anything

        if (!TryComp<GrappledComponent>(victim, out var victimComp))
            return false; // Somehow not a grappled target

        ReleaseGrapple((grappler, grappler.Comp),
            (victim.Value, victimComp),
            manualRelease: manualRelease,
            cleanupPulling: true);

        return true;
    }

    /// <summary>
    /// Handles applying the effects of grappling.
    /// Optionally starts a pulling action.
    /// </summary>
    /// <param name="grappler">Entity starting the grapple.</param>
    /// <param name="victim">Entity to be grappled.</param>
    /// <param name="startPulling">Whether this grapple should start a pulling joint.</param>
    private void StartGrapple(Entity<GrapplerComponent> grappler, EntityUid victim, bool startPulling = false)
    {
        // Throw the victim prone
        _standingStateSystem.Down(victim);

        // Ensure they have the grappled component for handling escapes and blocking movement.
        EnsureComp<GrappledComponent>(victim, out var grappled);
        grappled.Grappler = grappler;
        grappled.EscapeTime = grappler.Comp.EscapeTime;

        // Disable hands if requested
        TryDisableHands(grappler!, (victim, grappled));

        // Update the grappler's victim
        grappler.Comp.ActiveVictim = victim;
        Dirty(grappler);

        // Update any movement blocks that the grappler/grappled now have
        _actionBlockerSystem.UpdateCanMove(grappler);
        _actionBlockerSystem.UpdateCanMove(victim);

        // Optionally try and start pulling the target so both the grappler and the victim can't be
        // tugged away from one another.
        if (startPulling)
            _pullingSystem.TryStartPull(grappler, victim);

        _popupSystem.PopupEntity(
            Loc.GetString("grapple-start", ("part", grappler.Comp.GrapplingPart), ("victim", victim)),
            victim,
            grappler,
            PopupType.MediumCaution);
        _popupSystem.PopupEntity(
            Loc.GetString("grapple-start-victim", ("part", grappler.Comp.GrapplingPart), ("grappler", grappler)),
            victim,
            victim,
            PopupType.MediumCaution);

        _audioSystem.PlayPvs(grappler.Comp.GrappleSound, victim);

        _alerts.ShowAlert(grappler, grappler.Comp.GrappledAlert);
        _alerts.ShowAlert(victim, grappler.Comp.GrappledAlert);
    }

    /// <summary>
    /// Handles when a grappler starts to pull an entity, and attempts to start a grapple
    /// if they are in harm-mode.
    /// Validates the entity is valid for grappling.
    /// </summary>
    /// <param name="grappler">Entity attempting to start pulling.</param>
    /// <param name="args">Args for the event, notably the entity being pulled.</param>
    private void OnPullStarted(Entity<GrapplerComponent> grappler, ref PullStartedMessage args)
    {
        if (!TryComp<CombatModeComponent>(grappler, out var combatMode) ||
            !combatMode.IsInCombatMode)
            return; // Not in harm mode, this is just a regular pull

        // We rely on the pulling system to handle the joints for us, which means when we
        // start a grapple and request a pull, we end up here.
        if (grappler.Comp.ActiveVictim == args.PulledUid)
            return;

        TryStartGrapple(grappler.AsNullable(), args.PulledUid);
    }

    /// <summary>
    /// Handles when a grappler stops pulling an entity.
    /// If they have an active victim, they will be dropped.
    /// </summary>
    /// <param name="grappler">Grappler attempting to stop pulling an entity.</param>
    /// <param name="args">Args for the event.</param>
    private void OnPullStopped(Entity<GrapplerComponent> grappler, ref PullStoppedMessage args)
    {
        if (!grappler.Comp.ActiveVictim.HasValue)
            return; // No one to release from the grapple

        // We rely on the pulling system to handle the joints for us, which means when we
        // stop a grapple and clean up the grapple, we can end up here.
        if (grappler.Comp.ActiveVictim == args.PulledUid)
            return;

        ReleaseGrapple(grappler.AsNullable(), manualRelease: true);
    }

    /// <summary>
    /// Attempts to disable hands as requested by the Grappler's component.
    /// Disabled hands are stored on the GrappledComponent and will be re-enabled.
    /// </summary>
    /// <param name="grappler">Entity which has become grappled.</param>
    /// <param name="victim">Victim which has become grappled.</param>
    private void TryDisableHands(Entity<GrapplerComponent> grappler, Entity<GrappledComponent> victim)
    {
        if (!TryComp<HandsComponent>(victim, out var hands))
            return; // This victim has no hands

        if (grappler.Comp.HandDisabling == HandDisabling.None)
            return; // Nothing left to do

        List<DisabledHand> toDisable = [];
        switch (grappler.Comp.HandDisabling)
        {
            case HandDisabling.None:
                return;
            case HandDisabling.SingleRandom:
                var randomHand = _random.Next(0, hands.Count);
                var handName = hands.SortedHands[randomHand];
                var handComp = hands.Hands[handName];
                toDisable.Add(new DisabledHand(handName, handComp.Location));
                break;
            case HandDisabling.All:
                foreach (var hand in _handsSystem.EnumerateHands(victim, hands))
                {
                    toDisable.Add(new DisabledHand(hand.Name, hand.Location));
                }

                break;
        }

        // Store and remove the hand from the victim
        foreach (var disabledHand in toDisable)
        {
            _handsSystem.RemoveHand(victim, disabledHand.Name, hands);
        }

        victim.Comp.DisabledHands = toDisable;
    }

    /// <summary>
    /// Handles when the Grappled component is removed from an entity.
    /// Re-enables the hand, but does not update their movement.
    /// This must be done by a separate call AFTER this component is removed.
    /// </summary>
    /// <param name="grappled">Entity which is no longer grappled.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGrappledShutdown(Entity<GrappledComponent> grappled, ref ComponentShutdown args)
    {
        if (grappled.Comp.DisabledHands != null)
        {
            // Re-enable the hands and put it back on the correct location
            foreach (var hand in grappled.Comp.DisabledHands)
            {
                _handsSystem.AddHand(grappled, hand.Name, hand.Location);
            }
        }
    }

    /// <summary>
    /// Handles when a grappled entity attempts to move, and allows them to start
    /// to wriggling free of the grapple.
    /// Raises a DoAfter for the user to complete their escape.
    /// </summary>
    /// <param name="grappled">Entity attempting to move.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGrappledMove(Entity<GrappledComponent> grappled, ref MoveInputEvent args)
    {
        if (!args.HasDirectionalMovement)
            return;

        BeginEscapeAttempt(grappled);
    }

    /// <summary>
    /// Handles starting an escape attempt.
    /// </summary>
    /// <param name="grappled">Entity beginning the escape.</param>
    private void BeginEscapeAttempt(Entity<GrappledComponent> grappled)
    {
        if (!TryComp<GrapplerComponent>(grappled.Comp.Grappler, out var grappler))
            return; // Somehow grappled by a non-grappler?

        var escapeDoAfter = new DoAfterArgs(
            EntityManager,
            grappled,
            grappled.Comp.EscapeTime,
            new GrappledEscapeDoAfter(),
            grappled)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = true
        };

        if (!_doAfterSystem.TryStartDoAfter(escapeDoAfter, out var doAfterId))
            return;

        grappled.Comp.DoAfterId = doAfterId;

        _popupSystem.PopupEntity(Loc.GetString("grapple-start-escaping", ("victim", grappled)),
            grappled,
            grappled.Comp.Grappler,
            PopupType.MediumCaution);
        _popupSystem.PopupEntity(Loc.GetString("grapple-start-escaping-victim", ("part", grappler.GrapplingPart)),
            grappled,
            grappled,
            PopupType.MediumCaution);
    }

    /// <summary>
    /// Handles when the escape doafter is successful, which removes the grappling from the entity.
    /// </summary>
    /// <param name="grappled">Entity which has finished escaping from the grapple.</param>
    /// <param name="args">Args for the event.</param>
    private void OnEscapeDoAfter(Entity<GrappledComponent> grappled, ref GrappledEscapeDoAfter args)
    {
        if (args.Cancelled)
            return; // Was manually cancelled in some way

        if (!TryComp<GrapplerComponent>(grappled.Comp.Grappler, out var grappler))
            return; // Somehow not a grappler for this entity?

        ReleaseGrapple((grappled.Comp.Grappler, grappler), grappled, manualRelease: false, cleanupPulling: true);
    }

    /// <summary>
    /// Handles when a grappled player clicks the grappled alert, beginning an escape attempt.
    /// </summary>
    /// <param name="grappled">Grappled player entity which triggered the escape attempt.</param>
    /// <param name="args">Args for the event.</param>
    private void OnEscapeGrappledAlert(Entity<GrappledComponent> grappled, ref EscapeGrappleAlertEvent args)
    {
        BeginEscapeAttempt(grappled);
    }

    /// <summary>
    /// Handles when a grappler player clicks the grappled alert, beginning an escape attempt.
    /// </summary>
    /// <param name="grappled">Grappler player entity which has stopped grappling.</param>
    /// <param name="args">Args for the event.</param>
    private void OnEscapeGrapplerAlert(Entity<GrapplerComponent> grappler, ref EscapeGrappleAlertEvent args)
    {
        ReleaseGrapple(grappler.AsNullable(), manualRelease: true);
    }

    /// <summary>
    /// Handles releasing the effects of a grapple from both entities.
    /// </summary>
    /// <param name="grappler">Entity performing the grapple.</param>
    /// <param name="victim">Victim who has escaped or been released from the grapple</param>
    private void ReleaseGrapple(Entity<GrapplerComponent> grappler,
        Entity<GrappledComponent> victim,
        bool manualRelease = false,
        bool cleanupPulling = false)
    {
        // Ensure any pulling is cleaned up
        if (cleanupPulling && TryComp<PullableComponent>(victim, out var pulledComp))
            _pullingSystem.TryStopPull(victim, pulledComp);

        // Inform the grappler that their victim is now free and they can move, updating the cooldown as well.
        grappler.Comp.ActiveVictim = null;
        grappler.Comp.CooldownEnd = _gameTiming.CurTime + grappler.Comp.Cooldown;
        Dirty(grappler);
        _actionBlockerSystem.UpdateCanMove(grappler);

        // If this was a manul release by the grappler, we should cancel the doafter they have in progress, if any.
        if (manualRelease)
        {
            if (victim.Comp.DoAfterId.HasValue)
            {
                _doAfterSystem.Cancel(victim.Comp.DoAfterId.Value);
            }

            _popupSystem.PopupEntity(
                Loc.GetString("grapple-manual-release", ("victim", victim), ("part", grappler.Comp.GrapplingPart)),
                victim,
                grappler,
                PopupType.Medium);
            _popupSystem.PopupEntity(Loc.GetString("grapple-manual-release-victim",
                    ("grappler", grappler),
                    ("part", grappler.Comp.GrapplingPart)),
                victim,
                victim,
                PopupType.Medium);
        }
        else
        {
            _popupSystem.PopupEntity(Loc.GetString("grapple-finished-escaping",
                    ("victim", victim),
                    ("part", grappler.Comp.GrapplingPart)),
                victim,
                grappler,
                PopupType.MediumCaution);
            _popupSystem.PopupEntity(
                Loc.GetString("grapple-finished-escaping-victim", ("part", grappler.Comp.GrapplingPart)),
                victim,
                victim,
                PopupType.MediumCaution);
        }

        // Cleanup the grappling on the victim
        RemComp<GrappledComponent>(victim);
        _actionBlockerSystem.UpdateCanMove(victim); // Must be done AFTER the component is removed.

        _alerts.ClearAlert(grappler, grappler.Comp.GrappledAlert);
        _alerts.ClearAlert(victim, grappler.Comp.GrappledAlert);
    }
}
