using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._DV.Grappling.Components;
using Content.Shared._DV.Grappling.EntitySystems;
using Content.Shared._DV.Grappling.Events;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.Grappling.EntitySystems;

/// <summary>
/// Service side handling of grappling
/// </summary>
public sealed partial class GrapplingSystem : SharedGrapplingSystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StandingStateSystem _standingStateSystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private ProtoId<TagPrototype> _grappleTargetId = "GrappleTarget";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrapplerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<GrapplerComponent, PullStoppedMessage>(OnPullStopped);

        SubscribeLocalEvent<GrappledComponent, ComponentShutdown>(OnGrappledShutdown);
        SubscribeLocalEvent<GrappledComponent, MoveInputEvent>(OnGrappledMove);
        SubscribeLocalEvent<GrappledComponent, GrappledEscapeDoAfter>(OnEscapeDoAfter);
    }

    /// <summary>
    /// Begins grappling a victim, rendering them unable to move and possibly
    /// removing their ability to use their hands.
    /// </summary>
    /// <param name="grappler">Grappler entity to begin begin grappling</param>
    /// <param name="victim">Entity to be grappled.</param>
    public void StartGrapple(Entity<GrapplerComponent?> grappler, EntityUid victim)
    {
        if (!Resolve(grappler, ref grappler.Comp))
            return;

        if (!_actionBlockerSystem.CanInteract(grappler, victim))
            return;

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
    }

    /// <summary>
    /// Releases a victim from a grapple allowing them to move again and returning any
    /// hands that were disabled.
    /// </summary>
    /// <param name="grappler">Entity, which was performing the grapple.</param>
    /// <param name="manualRelease">Whether this release is considered by the grappler, or by the grappled.</param>
    public void ReleaseGrapple(Entity<GrapplerComponent> ent, bool manualRelease = false)
    {
        var victim = ent.Comp.ActiveVictim;
        if (!victim.HasValue)
            return; // Not grappling anything

        if (!TryComp<GrappledComponent>(victim, out var victimComp))
            return; // Somehow not a grappled target

        ReleaseGrapple(ent, (victim.Value, victimComp), manualRelease: manualRelease);
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

        if (grappler.Comp.ActiveVictim.HasValue)
            return; // Can't grapple a second person

        var victim = args.PulledUid;
        if (!TryComp<TagComponent>(victim, out var tagComp) ||
            !_tagSystem.HasTag(tagComp, _grappleTargetId))
            return; // Not a valid target

        StartGrapple(grappler.AsNullable(), victim);
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

        ReleaseGrapple(grappler, true);
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

        ReleaseGrapple((grappled.Comp.Grappler, grappler), grappled);
    }

    /// <summary>
    /// Handles releasing the effects of a grapple from both entities.
    /// </summary>
    /// <param name="grappler">Entity performing the grapple.</param>
    /// <param name="victim">Victim who has escaped or been released from the grapple</param>
    private void ReleaseGrapple(Entity<GrapplerComponent> grappler,
        Entity<GrappledComponent> victim,
        bool manualRelease = false)
    {
        // Inform the grappler that their victim is now free and they can move
        grappler.Comp.ActiveVictim = null;
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
    }
}
