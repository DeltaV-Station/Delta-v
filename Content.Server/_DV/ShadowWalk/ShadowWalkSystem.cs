using Content.Shared._DV.Body;
using Content.Shared._DV.Light;
using Content.Shared._DV.ShadowWalk;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Server._DV.ShadowWalk;

/// <summary>
/// Scans for static blockers around shadow walkers and marks the ones bathed in darkness
/// as passable. Runs on the server only: server lighting is authoritative, the results are
/// networked via <see cref="ShadowWalkerComponent.PassableEntities"/>.
/// </summary>
public sealed class ShadowWalkSystem : SharedShadowWalkSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedLightReactiveSystem _lightReactive = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <summary>
    /// How far outside an object's AABB the walker's centre has to be before a no-longer-dark
    /// object turns solid again. Must be at least the walker's collision radius so it can
    /// never re-solidify around them; the walker is treated as a point (its centre) here.
    /// </summary>
    private const float UnstickMargin = 0.45f;

    /// <summary>
    /// How far outside an object's AABB its light level is sampled. Keeps the sample point
    /// (and the occlusion rays cast from it) outside the object's own fixture.
    /// </summary>
    private const float SampleSkin = 0.1f;

    private EntityQuery<FixturesComponent> _fixturesQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<LightLevelHealthComponent> _lightHealthQuery;
    private EntityQuery<MapGridComponent> _gridQuery;

    private readonly HashSet<EntityUid> _candidates = [];
    private readonly HashSet<EntityUid> _passable = [];

    public override void Initialize()
    {
        base.Initialize();

        _fixturesQuery = GetEntityQuery<FixturesComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _lightHealthQuery = GetEntityQuery<LightLevelHealthComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ShadowWalkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;
            comp.NextUpdate = _timing.CurTime + comp.UpdateFrequency;

            UpdatePassable((uid, comp));
        }
    }

    private void UpdatePassable(Entity<ShadowWalkerComponent> ent)
    {
        var (uid, comp) = ent;

        // Darkness is whatever the walker heals in, if it's the kind that heals in darkness.
        var threshold = _lightHealthQuery.TryComp(uid, out var lightHealth)
            ? lightHealth.DarkThreshold
            : comp.DarkThreshold;

        // Anything our hard fixtures can't collide with is irrelevant.
        var ourMask = 0;
        if (_fixturesQuery.TryComp(uid, out var ourFixtures))
        {
            foreach (var fixture in ourFixtures.Fixtures.Values)
            {
                if (fixture.Hard)
                    ourMask |= fixture.CollisionMask;
            }
        }

        _passable.Clear();
        _candidates.Clear();
        _lookup.GetEntitiesInRange(uid, comp.Range, _candidates, LookupFlags.Static | LookupFlags.Approximate);

        var worldPos = _transform.GetWorldPosition(uid);

        foreach (var other in _candidates)
        {
            if (!_physicsQuery.TryComp(other, out var body))
                continue;
            if (body.BodyType != BodyType.Static || !body.CanCollide || !body.Hard)
                continue;
            // Never phase through the grid itself.
            if (_gridQuery.HasComp(other))
                continue;
            if (!_fixturesQuery.TryComp(other, out var fixtures))
                continue;

            var blocks = false;
            foreach (var fixture in fixtures.Fixtures.Values)
            {
                if (fixture.Hard && (fixture.CollisionLayer & ourMask) != 0)
                {
                    blocks = true;
                    break;
                }
            }
            if (!blocks)
                continue;

            // Sample the light where we'd actually enter: the nearest point on the object's
            // bounds, just outside its fixture. An occlusion ray can't hit the shape it starts
            // inside of, so sampling the object's centre would let light from the far side shine
            // straight through its own body — a wall between a lit room and a dark corridor must
            // count as dark when approached from the corridor, since that's the face the player
            // can see.
            var samplePoint = _lookup.GetWorldAABB(other).Enlarged(SampleSkin).ClosestPoint(worldPos);
            if (_lightReactive.GetLightLevelAtPosition(other, samplePoint) < threshold)
                _passable.Add(other);
        }

        // If something we're currently inside of got lit up (or left scan range), keep it
        // passable until we're fully clear of it so we can never be trapped inside a wall.
        foreach (var old in comp.PassableEntities)
        {
            if (_passable.Contains(old) || Deleted(old))
                continue;

            if (_lookup.GetWorldAABB(old).Enlarged(UnstickMargin).Contains(worldPos))
                _passable.Add(old);
        }

        if (comp.PassableEntities.SetEquals(_passable))
            return;

        SetPassable(ent, _passable);
    }
}
