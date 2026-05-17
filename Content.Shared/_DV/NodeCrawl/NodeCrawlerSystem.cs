using System.Numerics;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.NodeCrawl;

public sealed class NodeCrawlerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NodeCrawlerComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<NodeCrawlerComponent> ent, ref GetVerbsEvent<InnateVerb> args)
    {
        var target = args.Target;
        if (!HasComp<NodeCrawlComponent>(target))
            return;

        args.Verbs.Add(new InnateVerb()
        {
            Act = () =>
            {
                ent.Comp.Node = target;
                Dirty(ent);
            },
            Text = "node crawl",
        });
    }

    public bool TryTick(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent> sharedMover)
    {
        if (!TryComp<NodeCrawlerComponent>(sharedMover, out var crawler) || crawler.Node is null)
            return false;

        Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> mover = (sharedMover.Owner, sharedMover.Comp1, sharedMover.Comp2, sharedMover.Comp3, crawler);

        if (mover.Comp4.TargetNode is { } target)
            OngoingMovement(mover, target);
        else
            StartMovement(mover);

        return true;
    }

    private void StartMovement(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> mover)
    {
        if (GetDestination(mover, mover.Comp1.HeldMoveButtons) is not { } target)
            return;

        mover.Comp4.TargetNode = target;
        Dirty(mover, mover.Comp4);
    }

    private void StopMovement(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> mover)
    {
        _physics.SetLinearVelocity(mover, Vector2.Zero, body: mover.Comp2);
        _physics.SetAngularVelocity(mover, 0, body: mover.Comp2);
    }

    private void OngoingMovement(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> mover,
        EntityUid target)
    {
        var speed = MoveSpeed(mover);

        if (ReachedDestination(mover, target, speed))
        {
            StopMovement(mover);
            mover.Comp4.Node = target;
            mover.Comp4.TargetNode = null;
            Dirty(mover, mover.Comp4);
            return;
        }

        UpdateMovement(mover, target, speed);
    }

    private float MoveSpeed(Entity<InputMoverComponent> mover)
    {
        var moveSpeed = CompOrNull<MovementSpeedModifierComponent>(mover);

        var walkSpeed = moveSpeed?.CurrentWalkSpeed ?? MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
        var sprintSpeed = moveSpeed?.CurrentSprintSpeed ?? MovementSpeedModifierComponent.DefaultBaseSprintSpeed;
        return mover.Comp.Sprinting ? sprintSpeed : walkSpeed;
    }

    private void UpdateMovement(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> mover,
        EntityUid target,
        float speed)
    {
        var delta = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(mover.Comp3);

        var facing = Angle.FromWorldVec(delta);
        _transform.SetWorldRotation(mover.Comp3, facing);

        var velocity = delta;
        velocity.Normalize();
        velocity *= speed;

        _physics.SetLinearVelocity(mover, velocity, body: mover.Comp2);
        _physics.SetAngularVelocity(mover, 0, body: mover.Comp2);
    }

    private bool ReachedDestination(
        Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> mover,
        EntityUid target,
        float speed)
    {
        var delta = _transform.GetWorldPosition(mover.Comp3) - _transform.GetWorldPosition(target);
        return delta.EqualsApprox(Vector2.Zero, speed * 0.01f);
    }

    private EntityUid? GetDestination(Entity<InputMoverComponent, PhysicsComponent, TransformComponent, NodeCrawlerComponent> ent, MoveButtons buttons)
    {
        if ((buttons & MoveButtons.AnyDirection) == 0)
            return null;

        var target = _mover.DirVecForButtons(buttons);
        target = _mover.GetParentGridAngle(ent.Comp1).RotateVec(target);
        if (ent.Comp4.Node is not { } node || !Exists(node) || !TryComp<NodeCrawlComponent>(node, out var nodeCrawl))
            return null;

        var nodeXform = Transform(node);
        var nodeWorld = _transform.GetWorldPosition(nodeXform);
        var smallestTarget = EntityUid.Invalid;
        var largestDot = 0d;

        foreach (var reachable in nodeCrawl.ReachableNodes)
        {
            var reachableXform = Transform(reachable);
            var reachableWorld = _transform.GetWorldPosition(reachableXform);
            var delta = reachableWorld - nodeWorld;
            delta.Normalize();

            var deltaTargetDot = Vector2.Dot(delta, target);

            if (deltaTargetDot < largestDot)
                continue;

            smallestTarget = reachable;
            largestDot = deltaTargetDot;
        }

        if (!smallestTarget.Valid || largestDot <= Math.Cos(ent.Comp4.RequiredAngle))
            return null;

        return smallestTarget;
    }
}
