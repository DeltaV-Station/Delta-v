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
using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Mobs;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Content.Shared.Standing;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.Physics;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Grappling.EntitySystems;

/// <summary>
/// Server side handling of grappling
/// </summary>
public sealed partial class GrapplingSystem : SharedGrapplingSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly InteractionSystem _interaction = default!;
    [Dependency] private readonly JointSystem _joint = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtual = default!;

    private ProtoId<TagPrototype> _grappleTargetId = "GrappleTarget";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrapplerComponent, StartPullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<GrapplerComponent, EscapeGrappleAlertEvent>(OnEscapeGrapplerAlert);
        SubscribeLocalEvent<GrapplerComponent, MobStateChangedEvent>(OnGrapplerStateChanged);

        SubscribeLocalEvent<GrappledComponent, MobStateChangedEvent>(OnGrappledStateChanged);
        SubscribeLocalEvent<GrappledComponent, MoveInputEvent>(OnGrappledMove);
        SubscribeLocalEvent<GrappledComponent, GrappledEscapeDoAfter>(OnEscapeDoAfter);
        SubscribeLocalEvent<GrappledComponent, EscapeGrappleAlertEvent>(OnEscapeGrappledAlert);
        SubscribeLocalEvent<GrappledComponent, EntInsertedIntoContainerMessage>(OnCuffsInsertedIntoContainer);
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
            return false; // Can't grapple more than one target

        if (_gameTiming.CurTime < grappler.Comp.CooldownEnd)
            return false; // Cooldown on the grapple is not over yet

        if (!HasComp<GrappleTargetComponent>(victim))
            return false;  // Not a valid target

        return _actionBlocker.CanInteract(grappler, victim);
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

        if (!_interaction.InRangeUnobstructed(grappler.Owner, victim))
            return false;

        StartGrapple((grappler, grappler.Comp), victim);
        return true;
    }

    /// <summary>
    /// Releases a victim from a grapple allowing them to move again and returning any
    /// hands that were disabled.
    /// </summary>
    /// <param name="grappler">Entity, which was performing the grapple.</param>
    /// <param name="manualRelease">Whether this release is performed by the grappler, or by the grappled.</param>
    /// <returns>True if the grapple was released, false otherwise.</returns>
    public bool ReleaseGrapple(Entity<GrapplerComponent?> grappler, bool manualRelease = false)
    {
        if (!Resolve(grappler, ref grappler.Comp))
            return false;

        if (grappler.Comp.ActiveVictim is not { } victim)
            return false; // Not grappling anything

        if (!TryComp<GrappledComponent>(victim, out var victimComp))
            return false; // Somehow not a grappled target

        ReleaseGrapple((grappler, grappler.Comp),
            (victim, victimComp),
            manualRelease: manualRelease);

        return true;
    }

    /// <summary>
    /// Handles applying the effects of grappling.
    /// Optionally starts a pulling action.
    /// </summary>
    /// <param name="grappler">Entity starting the grapple.</param>
    /// <param name="victim">Entity to be grappled.</param>
    private void StartGrapple(Entity<GrapplerComponent> grappler, EntityUid victim)
    {
        // Throw the victim and grappler (if requested) prone
        _standingState.Down(victim);

        if (grappler.Comp.ProneOnGrapple)
            _standingState.Down(grappler);

        // Ensure they have the grappled component for handling escapes and blocking movement.
        EnsureComp<GrappledComponent>(victim, out var grappled);
        grappled.Grappler = grappler;
        grappled.EscapeTime = grappler.Comp.EscapeTime;

        // Disable hands if requested
        DisableHands(grappler!, (victim, grappled));

        // Update the grappler's victim
        grappler.Comp.ActiveVictim = victim;
        grappler.Comp.PullJointId = $"grapple-joint-{GetNetEntity(victim)}";
        Dirty(grappler);

        // Update any movement blocks that the grappler/grappled now have
        _actionBlocker.UpdateCanMove(grappler);
        _actionBlocker.UpdateCanMove(victim);

        // Joint the two together so both the grappler and the victim can't be tugged away from one another.
        _joint.CreateDistanceJoint(grappler, victim, id: grappler.Comp.PullJointId);

        _popup.PopupEntity(
            Loc.GetString("grapple-start", ("part", grappler.Comp.GrapplingPart), ("victim", victim)),
            victim,
            grappler,
            PopupType.MediumCaution);
        _popup.PopupEntity(
            Loc.GetString("grapple-start-victim", ("part", grappler.Comp.GrapplingPart), ("grappler", grappler)),
            victim,
            victim,
            PopupType.MediumCaution);

        _audio.PlayPvs(grappler.Comp.GrappleSound, victim);

        _alerts.ShowAlert(grappler.Owner, grappler.Comp.GrappledAlert);
        _alerts.ShowAlert(victim, grappler.Comp.GrappledAlert);
    }

    /// <summary>
    /// Handles when a grappler attempts to start pulling an entity.
    /// If they have an existing target, they will drop them.
    /// If they do NOT have a target then they will attempt to grapple them if they are in combat mode.
    /// Will stop the pull attempt if this system handles it via a grapple.
    /// </summary>
    /// <param name="grappler">Entity attempting to start pulling.</param>
    /// <param name="args">Args for the event, notably the entity being pulled.</param>
    private void OnPullAttempt(Entity<GrapplerComponent> grappler, ref StartPullAttemptEvent args)
    {
        if (grappler.Comp.ActiveVictim.HasValue)
        {
            if (ReleaseGrapple(grappler.AsNullable(), manualRelease: true))
                args.Cancel();
        }
        else
        {
            if (!TryComp<CombatModeComponent>(grappler, out var combatMode) ||
                    !combatMode.IsInCombatMode)
                return; // Not in harm mode, this is just a regular pull

            if (TryStartGrapple(grappler.AsNullable(), args.Pulled))
                args.Cancel(); // We've handled it.
        }
    }

    /// <summary>
    /// Attempts to disable hands as requested by the Grappler's component.
    /// Disabled hands are stored on the GrappledComponent and will be re-enabled.
    /// </summary>
    /// <param name="grappler">Entity performing the grapple.</param>
    /// <param name="victim">Victim which has become grappled.</param>
    private void DisableHands(Entity<GrapplerComponent> grappler, Entity<GrappledComponent> victim)
    {
        if (!TryComp<HandsComponent>(victim, out var hands))
            return; // This victim has no hands

        if (grappler.Comp.HandDisabling == HandDisabling.None)
            return; // Nothing left to do

        var toBlock = new List<string>(2); // Most entities have a maximum of two hands, so default to a list of two hands
        switch (grappler.Comp.HandDisabling)
        {
            case HandDisabling.None:
                return;
            case HandDisabling.SingleRandom:
                var randomHand = _random.Next(0, hands.Count);
                var handName = hands.SortedHands[randomHand];
                var handComp = hands.Hands[handName];
                toBlock.Add(handName);
                break;
            case HandDisabling.SingleActive:
                var activeHand = _hands.GetActiveHand((victim, hands));
                if (activeHand != null)
                    toBlock.Add(activeHand!);
                break;
            case HandDisabling.All:
                foreach (var hand in _hands.EnumerateHands((victim, hands)))
                {
                    toBlock.Add(hand);
                }
                break;
        }

        foreach (var hand in toBlock)
        {
            if (_virtual.TrySpawnVirtualItemInHand(grappler, victim, out var virtItem, dropOthers: true, hand))
            {
                EnsureComp<UnremoveableComponent>(virtItem.Value);
                victim.Comp.DisabledHands.Add(hand);
            }
        }
    }


    /// <summary>
    /// Attempts to enable hands that were previously disabled,
    /// as requested by the Grappler's component.
    /// </summary>
    /// <param name="grappler">Entity which was performing grapple.</param>
    /// <param name="victim">Victim which had become grappled.</param>
    private void EnableHands(Entity<GrapplerComponent> grappler, Entity<GrappledComponent> victim)
    {
        if (!TryComp<HandsComponent>(victim, out var hands))
            return; // This victim has no hands

        if (grappler.Comp.HandDisabling == HandDisabling.None)
            return; // Nothing left to do

        _virtual.DeleteInHandsMatching(victim, grappler);

        // Because the virtual items are queued for deletion, but not actually removed from hands yet,
        // we remove the component that makes them "unremovable", so that other systems like cuffs
        // can add virtual items immediately.
        foreach (var handName in victim.Comp.DisabledHands)
        {
            if (!_hands.TryGetHand((victim, hands), handName, out var hand))
                continue;


            if (!_hands.TryGetHeldItem((victim, hands), handName, out var item))
                continue;

            if (!item.HasValue)
                continue;

            RemComp<UnremoveableComponent>(item.Value);
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

        if (!_doAfter.TryStartDoAfter(escapeDoAfter, out var doAfterId))
            return;

        grappled.Comp.DoAfterId = doAfterId;

        _popup.PopupEntity(Loc.GetString("grapple-start-escaping", ("victim", grappled)),
            grappled,
            grappled.Comp.Grappler,
            PopupType.MediumCaution);
        _popup.PopupEntity(Loc.GetString("grapple-start-escaping-victim", ("part", grappler.GrapplingPart)),
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

        ReleaseGrapple((grappled.Comp.Grappler, grappler), grappled, manualRelease: false);
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
    /// Handles when an entity is inserted into a container on the grappled entity.
    /// Specifically checks for cuffs being added, which will cause a release of the grapple.
    /// </summary>
    /// <param name="grappled">Entity which has had an item inserted into a container.</param>
    /// <param name="args">Args for the event.</param>
    private void OnCuffsInsertedIntoContainer(Entity<GrappledComponent> grappled, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<CuffableComponent>(grappled, out var cuffable))
            return; // Isn't cuffable so don't need to worry

        if (args.Container.ID != cuffable.Container?.ID)
            return; // Wasn't inserted into the cuff container

        // This entity is being cuffed, release the grapple and let hands be cuffed properly.
        ReleaseGrapple(grappled.Comp.Grappler);
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
    /// Handles when a grappler enters crit or dies while holding a grappler, which will then release it.
    /// </summary>
    /// <param name="grappler">Grappler player entity which has entered crit or death.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGrapplerStateChanged(Entity<GrapplerComponent> grappler, ref MobStateChangedEvent args)
    {
        if (!grappler.Comp.ActiveVictim.HasValue)
            return; // Not actively grappling anything

        if (args.NewMobState == MobState.Critical ||
            args.NewMobState == MobState.Dead)
        {
            ReleaseGrapple(grappler.AsNullable(), manualRelease: true);
        }
    }

    /// <summary>
    /// Handles when a grappled entity enters crit or dies while being held by a grappler, releasing the
    /// grappler for them.
    /// </summary>
    /// <param name="grappled">Grappled entity which has entered crit or death.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGrappledStateChanged(Entity<GrappledComponent> grappled, ref MobStateChangedEvent args)
    {
        if (grappled.Comp.Grappler == EntityUid.Invalid)
            return;

        if (args.NewMobState == MobState.Critical ||
            args.NewMobState == MobState.Dead)
        {
            ReleaseGrapple(grappled.Comp.Grappler, manualRelease: true);
        }
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
        // Ensure any jointing is cleaned up
        if (grappler.Comp.PullJointId != null)
        {
            _joint.RemoveJoint(grappler, grappler.Comp.PullJointId);
            grappler.Comp.PullJointId = null;
        }

        // Inform the grappler that their victim is now free and they can move, updating the cooldown as well.
        grappler.Comp.ActiveVictim = null;
        grappler.Comp.CooldownEnd = _gameTiming.CurTime + grappler.Comp.Cooldown;
        Dirty(grappler);
        _actionBlocker.UpdateCanMove(grappler);

        // Clean up the hold on their hands we have
        EnableHands(grappler, victim);

        // If this was a manul release by the grappler, we should cancel the doafter they have in progress, if any.
        if (manualRelease)
        {
            if (victim.Comp.DoAfterId.HasValue)
            {
                _doAfter.Cancel(victim.Comp.DoAfterId.Value);
            }

            _popup.PopupEntity(
                Loc.GetString("grapple-manual-release", ("victim", victim), ("part", grappler.Comp.GrapplingPart)),
                victim,
                grappler,
                PopupType.Medium);
            _popup.PopupEntity(Loc.GetString("grapple-manual-release-victim",
                    ("grappler", grappler),
                    ("part", grappler.Comp.GrapplingPart)),
                victim,
                victim,
                PopupType.Medium);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("grapple-finished-escaping",
                    ("victim", victim),
                    ("part", grappler.Comp.GrapplingPart)),
                victim,
                grappler,
                PopupType.MediumCaution);
            _popup.PopupEntity(
                Loc.GetString("grapple-finished-escaping-victim", ("part", grappler.Comp.GrapplingPart)),
                victim,
                victim,
                PopupType.MediumCaution);
        }

        // Cleanup the grappling on the victim
        RemComp<GrappledComponent>(victim);
        _actionBlocker.UpdateCanMove(victim); // Must be done AFTER the component is removed.



        // Automatically get the grappler back up
        if (grappler.Comp.ProneOnGrapple && TryComp<StandingStateComponent>(grappler, out var standingState) && _standingState.IsDown((grappler, standingState)))
            _standingState.Stand(grappler);

        _alerts.ClearAlert(grappler.Owner, grappler.Comp.GrappledAlert);
        _alerts.ClearAlert(victim.Owner, grappler.Comp.GrappledAlert);
    }
}
