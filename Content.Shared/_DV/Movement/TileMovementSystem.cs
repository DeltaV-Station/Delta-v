using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._DV.Movement;

public sealed class TileMovementSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<TileMovementComponent> _query;
    private EntityQuery<FixturesComponent> _fixturesQuery;
    private EntityQuery<InputMoverComponent> _moverQuery;
    private EntityQuery<MobMoverComponent> _mobMoverQuery;
    private EntityQuery<MovementSpeedModifierComponent> _modifierQuery;
    private EntityQuery<NoRotateOnMoveComponent> _noRotQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<PullerComponent> _pullerQuery;
    private EntityQuery<RelayInputMoverComponent> _relayQuery;

    private HashSet<EntityUid> _ticked = new();

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<TileMovementComponent>();
        _fixturesQuery = GetEntityQuery<FixturesComponent>();
        _moverQuery = GetEntityQuery<InputMoverComponent>();
        _mobMoverQuery = GetEntityQuery<MobMoverComponent>();
        _modifierQuery = GetEntityQuery<MovementSpeedModifierComponent>();
        _noRotQuery = GetEntityQuery<NoRotateOnMoveComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _pullerQuery = GetEntityQuery<PullerComponent>();
        _relayQuery = GetEntityQuery<RelayInputMoverComponent>();

        SubscribeLocalEvent<TileMovementComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TileMovementComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<TileMovementComponent, PullStoppedMessage>(OnPullStopped);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _ticked.Clear();
    }

    private void OnMapInit(Entity<TileMovementComponent> ent, ref MapInitEvent args)
    {
        if (GetTarget(ent) is not {} target)
            return;

        // when adding tile movement immediately move them to the tile center
        StartSlideTo(target, target.Comp4.LocalPosition);
        UpdateSlide(target);
    }

    private void OnPullStarted(Entity<TileMovementComponent> ent, ref PullStartedMessage args)
    {
        var target = args.PulledUid;
        if (ent.Owner != args.PullerUid || !_mobMoverQuery.HasComp(target))
            return;

        // if you have tile movement and pull a mob, it gets tile movement too temporarily.
        if (EnsureComp<TileMovementComponent>(target, out var comp))
            return;

        comp.Temporary = true;
        DirtyField(target, comp, nameof(TileMovementComponent.Temporary));
    }

    private void OnPullStopped(Entity<TileMovementComponent> ent, ref PullStoppedMessage args)
    {
        // only remove temporary tile movement when no longer pulled
        if (!ent.Comp.Temporary || ent.Owner != args.PulledUid)
            return;

        ent.Comp.Temporary = false;
        RemCompDeferred(ent, ent.Comp);
    }

    private Entity<InputMoverComponent, TileMovementComponent, PhysicsComponent, TransformComponent>? GetTarget(EntityUid player)
    {
        if (_relayQuery.TryComp(player, out var relay))
            player = relay.RelayEntity;

        if (!_query.TryComp(player, out var comp) ||
            !_moverQuery.TryComp(player, out var mover) ||
            !_physicsQuery.TryComp(player, out var physics))
            return null;

        return (player, mover, comp, physics, Transform(player));
    }

    private TimeSpan CurrentTime => _physics.EffectiveCurTime ?? _timing.CurTime;

    public bool HasTileMovement(EntityUid? uid)
    {
        return _query.HasComp(uid);
    }

    /// <summary>
    /// Tries to process a tick of tile movement for a mover.
    /// </summary>
    /// <param name="player">The player moving a mob</param>
    /// <param name="target">The movement target if not the player, i.e. a mech</param>
    /// <returns>True if it was handled</returns>
    public bool TryTick(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent> player,
        EntityUid? relaySource,
        ContentTileDefinition? tileDef,
        bool weightless,
        float frameTime)
    {
        if (!_query.TryComp(player, out var comp))
            return false;

        var ent = (player.Owner, player.Comp1, comp, player.Comp2, player.Comp3);

        // let client predict pulled movement so it looks good
        // this is needed since client only predicts its own movement
        if (_net.IsClient && _timing.IsFirstTimePredicted)
            RelayPulled(player, frameTime);

        var wasWeightless = comp.WasWeightlessLastTick;
        SetWeightless((player, comp), weightless);

        // no tiles in space...
        if (weightless || player.Comp2.BodyStatus != BodyStatus.OnGround)
        {
            EndSlide((player, comp, player.Comp2));
            SetButtons((player, comp), MoveButtons.None);
            return false;
        }

        // For smoothness' sake, if we just arrived on a grid after pixel moving in space then start
        // a slide towards the center of the tile we're on. It just ends up feeling better this way.
        if (wasWeightless)
        {
            StartSlideTo(ent, player.Comp3.LocalPosition);
            SetButtons((player, comp), MoveButtons.None);
            UpdateSlide(ent);
            return true;
        }

        // If we're not moving or trying to move, apply friction to existing velocity and then stop.
        var buttons = StripWalk(player.Comp1.HeldMoveButtons);
        if (buttons == MoveButtons.None && !comp.SlideActive)
        {
            var velocity = player.Comp2.LinearVelocity;
            var moveSpeed = _modifierQuery.CompOrNull(player);
            var friction = GetEntityFriction(player.Comp1, moveSpeed, tileDef);
            var minSpeed = moveSpeed?.MinimumFrictionSpeed ?? MovementSpeedModifierComponent.DefaultMinimumFrictionSpeed;
            _mover.Friction(minSpeed, frameTime, friction, ref velocity);

            _physics.SetLinearVelocity(player, velocity, body: player.Comp2);
            _physics.SetAngularVelocity(player, 0, body: player.Comp2);
            return true;
        }

        // Otherwise, begin tile movement.

        // Set WorldRotation so that our character is facing the way we're walking.
        if (!_noRotQuery.HasComp(player))
            Rotate((player, player.Comp1, comp, player.Comp3));

        // Play step sound.
        TryPlaySound((player, player.Comp1, player.Comp3), relaySource, tileDef);

        // If we're sliding possibly end the slide or continue it
        if (comp.SlideActive)
            TryEndSlide(ent);
        // Start sliding otherwise
        else if (buttons != MoveButtons.None)
            StartSlide(ent);

        return true;
    }

    private void RelayPulled(EntityUid puller, float frameTime)
    {
        if (_pullerQuery.CompOrNull(puller)?.Pulling is not {} player)
            return;

        if (GetTarget(player) is not {} target)
            return;

        // don't stack overflow if there's a pull circle A -> B -> C -> A ...
        if (!_ticked.Add(player))
            return;

        _mover.HandleMobMovement(target, frameTime);
    }

    public void SetWeightless(Entity<TileMovementComponent> player, bool weightless)
    {
        if (player.Comp.WasWeightlessLastTick == weightless)
            return;

        player.Comp.WasWeightlessLastTick = weightless;
        DirtyField(player, player.Comp, nameof(TileMovementComponent.WasWeightlessLastTick));
    }

    public void SetButtons(Entity<TileMovementComponent> player, MoveButtons buttons)
    {
        if (player.Comp.CurrentSlideMoveButtons == buttons)
            return;

        player.Comp.CurrentSlideMoveButtons = buttons;
        DirtyField(player, player.Comp, nameof(TileMovementComponent.CurrentSlideMoveButtons));
    }

    public void Rotate(Entity<InputMoverComponent, TileMovementComponent, TransformComponent> player)
    {
        if (!player.Comp2.SlideActive || player.Comp1.RelativeEntity is not {} rel)
            return;

        var relXform = Transform(rel);
        var delta = player.Comp2.Destination - player.Comp2.Origin;
        var worldRot = _transform.GetWorldRotation(relXform).RotateVec(delta).ToWorldAngle();
        _transform.SetWorldRotation(player.Comp3, worldRot);
    }

    public void TryPlaySound(
        Entity<InputMoverComponent, TransformComponent> player,
        EntityUid? relaySource,
        ContentTileDefinition? tileDef)
    {
        if (!_mobMoverQuery.TryComp(player, out var mobMover) ||
            !_mover.TryGetSound(false, player, player.Comp1, mobMover, player.Comp2, out var sound, tileDef))
        {
            return;
        }

        var soundModifier = player.Comp1.Sprinting ? 3.5f : 1.5f;
        var audioParams = sound.Params
            .WithVolume(sound.Params.Volume + soundModifier)
            .WithVariation(sound.Params.Variation ?? mobMover.FootstepVariation);
        _audio.PlayPredicted(sound, player, relaySource ?? player, audioParams);
    }

    public void TryEndSlide(Entity<InputMoverComponent, TileMovementComponent, PhysicsComponent, TransformComponent> player)
    {
        var speed = GetEntityMoveSpeed(player, player.Comp1.Sprinting);
        var buttons = StripWalk(player.Comp1.HeldMoveButtons);
        if (!ShouldSlideEnd(buttons, player.Comp4, player.Comp2, speed))
        {
            UpdateSlide(player);
            return;
        }

        // stop sliding now
        EndSlide((player, player.Comp2, player.Comp3));
        SetButtons((player, player.Comp2), buttons);
        if (buttons == MoveButtons.None)
        {
            ForceSnapToTile((player, player.Comp3, player.Comp4));
            return;
        }

        // if a button is still being held start sliding again immediately
        StartSlide(player);
        UpdateSlide(player);
    }

    public bool ShouldSlideEnd(MoveButtons buttons, TransformComponent xform, TileMovementComponent comp, float movementSpeed)
    {
        var minPressedTime = (1.05f / movementSpeed);
        // We need to stop the move once we are close enough. This isn't perfect, since it technically ends the move
        // 1 tick early in some cases. This is because there's a fundamental issue where because this is a physics-based
        // tile movement system, we sometimes find scenarios where on each tick of the physics system, the player is moved
        // back and forth across the destination in a loop. Thus, the tolerance needs to be set overly high so that it
        // reaches the distance once the physics body can move in a single tick.
        var destinationTolerance = movementSpeed * 0.01f;

        var reachedDestination = xform.LocalPosition.EqualsApprox(comp.Destination, destinationTolerance);
        var stoppedPressing = buttons != comp.CurrentSlideMoveButtons && (CurrentTime - comp.MovementKeyPressedAt) >= TimeSpan.FromSeconds(minPressedTime);
        return reachedDestination || stoppedPressing;
    }

    public void StartSlide(Entity<InputMoverComponent, TileMovementComponent, PhysicsComponent, TransformComponent> player)
    {
        var buttons = player.Comp1.HeldMoveButtons;
        var offset = _mover.DirVecForButtons(buttons);
        offset = player.Comp1.TargetRelativeRotation.RotateVec(offset);
        StartSlideTo(player, player.Comp4.LocalPosition + offset);
        SetButtons((player, player.Comp2), StripWalk(buttons));
    }

    // physics isnt used but it makes calling it a bit easier
    public void StartSlideTo(
        Entity<InputMoverComponent, TileMovementComponent, PhysicsComponent, TransformComponent> player,
        Vector2 dest)
    {
        player.Comp2.Origin = player.Comp4.LocalPosition;
        player.Comp2.Destination = SnapCoordinatesToTile(dest);
        player.Comp2.MovementKeyPressedAt = CurrentTime;
        DirtyField(player, player.Comp2, nameof(TileMovementComponent.Origin));
        DirtyField(player, player.Comp2, nameof(TileMovementComponent.Destination));
        DirtyField(player, player.Comp2, nameof(TileMovementComponent.MovementKeyPressedAt));

        // pull the pulled mob along if it currently has TileMovement
        if (_pullerQuery.CompOrNull(player)?.Pulling is not {} pulling || !_query.TryComp(pulling, out var pullingComp))
            return;

        if (GetTarget(pulling) is not {} pullTarget)
            return;

        // already set, don't stack overflow for pull circles
        if (pullingComp.Destination.EqualsApprox(player.Comp2.Origin, 0.01f))
            return;

        StartSlideTo(pullTarget, player.Comp2.Origin);
        UpdateSlide(pullTarget);
    }

    /// <summary>
    /// Forces the target entity's velocity based on where the player is moving to.
    /// </summary>
    public void UpdateSlide(Entity<InputMoverComponent, TileMovementComponent, PhysicsComponent, TransformComponent> player)
    {
        var parentRot = _mover.GetParentGridAngle(player.Comp1);
        var speed = GetEntityMoveSpeed(player, player.Comp1.Sprinting);

        // Determine velocity based on movespeed, and rotate it so that it's in the right direction.
        var velocity = player.Comp2.Destination - player.Comp4.LocalPosition;
        velocity.Normalize();
        velocity *= speed;
        velocity = parentRot.RotateVec(velocity);
        _physics.SetLinearVelocity(player, velocity, body: player.Comp3);
        _physics.SetAngularVelocity(player, 0, body: player.Comp3);
    }

    /// <summary>
    /// Kills the target entity's velocity and stops the current slide.
    /// </summary>
    public void EndSlide(Entity<TileMovementComponent, PhysicsComponent> player)
    {
        if (!player.Comp1.SlideActive)
            return;

        player.Comp1.MovementKeyPressedAt = null;
        DirtyField(player, player.Comp1, nameof(TileMovementComponent.MovementKeyPressedAt));

        _physics.SetLinearVelocity(player, Vector2.Zero, body: player.Comp2);
        _physics.SetAngularVelocity(player, 0, body: player.Comp2);
    }

    #region Helpers

    private float GetEntityMoveSpeed(EntityUid player, bool sprinting)
    {
        var moveSpeed = _modifierQuery.CompOrNull(player);
        return sprinting
            ? moveSpeed?.CurrentSprintSpeed ?? MovementSpeedModifierComponent.DefaultBaseSprintSpeed
            : moveSpeed?.CurrentWalkSpeed ?? MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
    }

    /// <summary>
    /// Returns the given local coordinates snapped to the center of the tile it is currently on.
    /// </summary>
    /// <param name="input">Given coordinates to snap.</param>
    /// <returns>The closest tile center to the input.<returns>
    private Vector2 SnapCoordinatesToTile(Vector2 input)
    {
        return new Vector2((int) Math.Floor(input.X) + 0.5f, (int) Math.Floor(input.Y) + 0.5f);
    }

    /// <summary>
    /// Instantly snaps/teleports an entity to the center of the tile it is currently standing on based on the
    /// given grid. Does not trigger collisions on the way there, but does trigger collisions after the snap.
    /// </summary>
    private void ForceSnapToTile(Entity<PhysicsComponent, TransformComponent> target)
    {
        var coords = target.Comp2.Coordinates.SnapToGrid(EntityManager, _map);
        _transform.SetCoordinates(target, target.Comp2, coords);
        _physics.WakeBody(target, body: target.Comp1);
    }

    private float GetEntityFriction(
        InputMoverComponent mover,
        MovementSpeedModifierComponent? moveSpeed,
        ContentTileDefinition? tileDef)
    {
        if (StripWalk(mover.HeldMoveButtons) != MoveButtons.None || moveSpeed == null)
        {
            return tileDef?.MobFriction ?? moveSpeed?.Friction ?? MovementSpeedModifierComponent.DefaultFriction;
        }
        return tileDef?.Friction ?? moveSpeed.FrictionNoInput;
    }

    /// <summary>
    /// Sets the walk value on the given MoveButtons input to zero.
    /// </summary>
    private MoveButtons StripWalk(MoveButtons input) => input & ~MoveButtons.Walk;

    #endregion
}
