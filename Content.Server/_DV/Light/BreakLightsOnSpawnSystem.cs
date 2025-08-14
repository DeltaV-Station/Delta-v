using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared._DV.Light;
using Content.Shared.Physics;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using System.Linq;
using System.Numerics;

namespace Content.Server._DV.Light;

public sealed partial class BreakLightsOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _lightSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly HashSet<Entity<PoweredLightComponent>> _lightsInRange = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BreakLightsOnSpawnComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BreakLightsOnSpawnComponent> entity, ref MapInitEvent args)
    {
        var xform = Transform(entity);
        var pos = _transform.GetWorldPosition(xform);
        // Get all light entities within the specified radius
        _lightsInRange.Clear();
        _lookup.GetEntitiesInRange(xform.Coordinates, entity.Comp.Radius, _lightsInRange);
        foreach (var light in _lightsInRange)
        {
            if (entity.Comp.LineOfSight) // If LoS is required, test it.
            {
                var lightPos = _transform.GetWorldPosition(light);
                var sqrDistance = Vector2.DistanceSquared(pos, lightPos);
                var ray = new CollisionRay(pos, (lightPos - pos).Normalized(), (int)CollisionGroup.Opaque);
                var hit = _physics.IntersectRay(_transform.GetMapId(entity.Owner), ray, MathF.Sqrt(sqrDistance) - 0.5f, returnOnFirstHit: true);
                if (hit.Any() && hit.First().Distance != 0)
                    continue;
            }

            // If we reach here, the light is unobstructed and within range, break it.
            _lightSystem.TryDestroyBulb(light, light.Comp);
        }
    }
}
