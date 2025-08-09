using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;

namespace Content.Shared._DV.Light;

public abstract class SharedLightReactiveSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Update(float frameTime)
    {

        var query = EntityQueryEnumerator<LightReactiveComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                return;
            comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(1);
            if (_mobState.IsDead(uid) && comp.OnlyWhileAlive)
                continue; // Don't apply damage / healing if the mob is dead
            // Get the light level at the entity's position
            comp.CurrentLightLevel = GetLightLevelForPoint(uid);
        }
    }

    public abstract List<Entity<SharedPointLightComponent>> GetLights();

    /// <summary>
    /// Gets the current light level of an entity.
    /// </summary>
    /// <remarks>
    /// This is a cached value that is updated periodically.
    /// I could add arguments to either: Force check if it's not a ReactiveComponent, or force update,
    /// but I don't need them so you don't get them. Add it if you want it.
    /// </remarks>
    public float GetLightLevel(EntityUid uid, bool forceUpdate = false)
    {
        if (TryComp(uid, out LightReactiveComponent? comp))
        {
            if (forceUpdate)
                comp.CurrentLightLevel = GetLightLevelForPoint(uid);
            return comp.CurrentLightLevel;
        }
        return 0.0f;
    }

    /// <summary>
    /// Gets the light level at a specific point in the world.
    /// Avoid calling this too often, as it can be expensive.
    /// </summary>
    public float GetLightLevelForPoint(EntityUid uid)
    {
        float val = 0.0f;
        // Get the current map entity so we can get a MapLightComponent from it if it has one
        var map = _transform.GetMap(uid);
        if (TryComp(map, out MapLightComponent? mapLight))
            val += (mapLight.AmbientLightColor.R + mapLight.AmbientLightColor.G + mapLight.AmbientLightColor.B) / 3f;
        var pos = _transform.GetWorldPosition(uid);

        foreach (var (lightUid, lightComp) in GetLights())
        {
            if (!lightComp.Enabled || lightComp.Deleted)
                continue; // Skip lights that are not enabled
            if (!lightComp.NetSyncEnabled)
                continue; // Skip lights that are not synced. This is used for ghosts and things.
            // Ensure we're on the same grid as the light source
            if (_transform.GetMap(lightUid) != map)
                continue;

            // Ensure we're within the light's radius.
            var lightPos = _transform.GetWorldPosition(lightUid);
            var sqrDistance = Vector2.DistanceSquared(pos, lightPos);
            if (sqrDistance > lightComp.Radius * lightComp.Radius)
                continue;

            if (sqrDistance < 0.01f)
            {
                // If we're right on top of the light, just add its full energy value.
                val += lightComp.Energy;
                continue;
            }

            // Collision ray check from the entity to the light source
            var ray = new CollisionRay(pos, (lightPos - pos).Normalized(), (int)CollisionGroup.Opaque);
            var hit = _physics.IntersectRay(_transform.GetMapId(uid), ray, MathF.Sqrt(sqrDistance) - 0.5f, returnOnFirstHit: true);
            if (hit.Any() && hit.First().Distance != 0)
                continue;
            // If we reach here, the light is unobstructed and within range, calculate a light value to add.
            val += lightComp.Energy * (1.0f - sqrDistance / (lightComp.Radius * lightComp.Radius));
        }


        return val;
    }
}
