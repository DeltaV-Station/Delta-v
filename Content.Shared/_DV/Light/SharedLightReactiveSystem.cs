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


    private EntityQuery<LightReactiveComponent> _lightReactive;

    public override void Initialize()
    {
        base.Initialize();
        _lightReactive = GetEntityQuery<LightReactiveComponent>();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<LightReactiveComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (comp.Manual)
                continue; // Don't auto update if it's manual
            if (_timing.CurTime < comp.NextUpdate)
                continue;
            comp.NextUpdate = _timing.CurTime + comp.UpdateFrequency;
            Dirty(uid, comp);
            // Technically this only runs once every X time so I'm leaving it as TryComp because I don't want to deal with the B/C
            if (_mobState.IsDead(uid) && comp.OnlyWhileAlive)
                continue; // Don't apply damage / healing if the mob is dead
            // Get the light level at the entity's position
            comp.CurrentLightLevel = GetLightLevelForPoint(uid, xform);
        }
    }

    public abstract HashSet<Entity<SharedPointLightComponent>> GetLights(EntityUid targetEntity);

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
        if (_lightReactive.TryComp(uid, out LightReactiveComponent? comp))
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
    public float GetLightLevelForPoint(EntityUid uid, TransformComponent? xform = null)
    {
        float val = 0.0f;
        // Get the current map entity so we can get a MapLightComponent from it if it has one
        var map = _transform.GetMap((uid, xform));
        if (TryComp(map, out MapLightComponent? mapLight))
            val += (mapLight.AmbientLightColor.R + mapLight.AmbientLightColor.G + mapLight.AmbientLightColor.B) / 3f;
        var pos = _transform.GetWorldPosition(uid);

        foreach (var (lightUid, lightComp) in GetLights(uid))
        {
            var energy = lightComp.Energy;
            var radius = lightComp.Radius;
            if (!lightComp.NetSyncEnabled)
            {
                // Try to use the GetLightEnergyEvent if we can't rely on it being network synced.
                var lightEnergyEvnt = new OnGetLightEnergyEvent();
                RaiseLocalEvent(lightUid, ref lightEnergyEvnt);
                energy = lightEnergyEvnt.LightEnergy;
                radius = lightEnergyEvnt.LightRadius;
                if (MathHelper.CloseTo(energy, 0f))
                    continue; // No light, no problem.
            }

            energy = MathF.Min(energy, 2f); // Clamp energy, to normalize strange values.

            // Ensure we're on the same grid as the light source
            if (_transform.GetMap(lightUid) != map)
                continue;

            // Ensure we're within the light's radius.
            var lightPos = _transform.GetWorldPosition(lightUid);
            var sqrDistance = Vector2.DistanceSquared(pos, lightPos);
            if (sqrDistance > radius * radius)
                continue;

            if (sqrDistance < 0.01f)
            {
                // If we're right on top of the light, just add its full energy value.
                val += energy;
                continue;
            }

            // Collision ray check from the entity to the light source
            var ray = new CollisionRay(pos, (lightPos - pos).Normalized(), (int)CollisionGroup.Opaque);
            var hit = _physics.IntersectRay(_transform.GetMapId((uid, xform)), ray, MathF.Sqrt(sqrDistance) - 0.5f, ignoredEnt: lightUid, returnOnFirstHit: true);
            var firstHit = hit.FirstOrDefault();
            if (firstHit.Distance != 0)
                continue;

            // Manual hack for cones.
            if (lightComp.MaskPath == "/Textures/Effects/LightMasks/cone.png")
            {
                var forward = _transform.GetWorldRotation(lightUid).RotateVec(new Vector2(0.0f, -1.0f));
                energy *= MathF.Max(0f, Vector2.Dot((pos - lightPos).Normalized(), forward));
            }
            else if (lightComp.MaskPath == "/Textures/Effects/LightMasks/double_cone.png")
            {
                var forward = _transform.GetWorldRotation(lightUid).RotateVec(new Vector2(0.0f, -1.0f));
                energy *= MathF.Abs(Vector2.Dot((pos - lightPos).Normalized(), forward));
            }

            // If we reach here, the light is unobstructed and within range, calculate a light value to add.
            val += energy * (1.0f - sqrDistance / (radius * radius));
        }


        return val;
    }
}

/// <summary>
/// Passed to unsync'd light sources to get the expected light energy.
/// ONLY called when NetSync is not enabled. Otherwise, uses the light directly.
/// </summary>
[ByRefEvent]
public record struct OnGetLightEnergyEvent()
{
    public float LightEnergy = 0f;
    public float LightRadius = 0f;
}
