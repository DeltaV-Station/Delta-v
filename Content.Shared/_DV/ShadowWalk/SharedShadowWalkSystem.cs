using Content.Shared._DV.Body;
using Content.Shared._DV.Light;
using Content.Shared.Projectiles;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._DV.ShadowWalk;

/// <summary>
/// Can walk through darkness freely.
/// </summary>
public sealed partial class SharedShadowWalkSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedLightReactiveSystem _lightReactive = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// How far outside a tagged object's AABB the walker's centre must be before the object is untagged (and so becomes solid again on the next collision).
    /// At least the walker's collision radius, so an object is never made solid while it still overlaps the walker.
    /// </summary>
    private const float UnstickMargin = 0.45f;

    /// <summary>
    /// Gamefeel. Non-walls get a bigger unstick margin so they stay unstick even if you clip into a wall. Prevents getting stuck in walls.
    /// </summary>
    private const float MovableUnstickMargin = 1f;

    private EntityQuery<LightLevelHealthComponent> _lightHealthQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<ProjectileComponent> _projectileQuery;

    private readonly List<EntityUid> _toRemove = [];

    public override void Initialize()
    {
        base.Initialize();

        _lightHealthQuery = GetEntityQuery<LightLevelHealthComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();

        SubscribeLocalEvent<ShadowWalkerComponent, PreventCollideEvent>(OnPreventCollide);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ShadowWalkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PassableEntities.Count == 0)
                continue;

            var worldPos = _transform.GetWorldPosition(uid);

            _toRemove.Clear();
            foreach (var other in comp.PassableEntities)
            {
                // Untag anything we've deleted or fully walked clear of; the next collision with it will re-check the light level from scratch.
                if (Deleted(other))
                {
                    _toRemove.Add(other);
                    continue;
                }

                var margin = UnstickMargin;
                // Non-statics get a bigger margin :)
                if (_physicsQuery.TryComp(other, out var body) && body.BodyType != BodyType.Static)
                    margin += MovableUnstickMargin;

                if (!_lookup.GetWorldAABB(other).Enlarged(margin).Contains(worldPos))
                    _toRemove.Add(other);
            }

            foreach (var other in _toRemove)
                comp.PassableEntities.Remove(other);
        }
    }

    private void OnPreventCollide(Entity<ShadowWalkerComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        // Only phase through hard blockers; sensor fixtures must keep triggering.
        if (!args.OurFixture.Hard || !args.OtherFixture.Hard)
            return;

        // Already phasing through this one: keep it passable until we've left it (pruned in
        // Update), so a light change mid-overlap can never trap us inside it.
        if (ent.Comp.PassableEntities.Contains(args.OtherEntity))
        {
            args.Cancelled = true;
            return;
        }

        if (!CanPhaseThrough(args.OtherEntity, args.OtherBody))
            return;

        // A fresh collision: only phase if the walker itself is currently in darkness.
        if (!InDarkness(ent))
            return;

        args.Cancelled = true;
        ent.Comp.PassableEntities.Add(args.OtherEntity);
    }

    private bool CanPhaseThrough(EntityUid other, PhysicsComponent otherBody)
    {
        if (otherBody.BodyType == BodyType.KinematicController)
            return false;
        // Bullets never pass or hit based on collision timing.
        if (_projectileQuery.HasComp(other))
            return false;

        return true;
    }

    private bool InDarkness(Entity<ShadowWalkerComponent> ent)
    {
        // Darkness is whatever the walker heals in, if it heals in darkness at all.
        var threshold = _lightHealthQuery.TryComp(ent, out var lightHealth)
            ? lightHealth.DarkThreshold
            : ent.Comp.DarkThreshold;

        var curTick = _timing.CurTick;
        if (ent.Comp.LastLightCheckTick != curTick)
        {
            ent.Comp.LastLightLevel = _lightReactive.GetLightLevelForPoint(ent.Owner);
            ent.Comp.LastLightCheckTick = curTick;
        }

        return ent.Comp.LastLightLevel < threshold;
    }
}
