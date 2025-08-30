using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared._DV.Abilities;
using Content.Shared.Actions;
using Content.Shared.Physics;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using System.Linq;
using System.Numerics;

namespace Content.Server._DV.Abilities;

public sealed partial class ShatterLightsAbilitySystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly HashSet<Entity<PoweredLightComponent>> _lightsInRange = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShatterLightsAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShatterLightsAbilityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShatterLightsAbilityComponent, ShatterLightsActionEvent>(OnShatterLightsAction);
    }

    private void OnMapInit(Entity<ShatterLightsAbilityComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ShatterLightsActionEntity, ent.Comp.ShatterLightsActionId);
        _actions.StartUseDelay(ent.Comp.ShatterLightsActionEntity);
    }

    private void OnShutdown(Entity<ShatterLightsAbilityComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.ShatterLightsActionEntity);
    }

    private void OnShatterLightsAction(Entity<ShatterLightsAbilityComponent> entity, ref ShatterLightsActionEvent args)
    {
        if (args.Handled)
            return;

        if (entity.Comp.AbilitySound != null)
            _audio.PlayPvs(entity.Comp.AbilitySound, entity);

        ShatterLightsAround(entity.Owner, entity.Comp.Radius, entity.Comp.LineOfSight);
        args.Handled = true;
    }

    public void ShatterLightsAround(EntityUid center, float range, bool lineOfSight)
    {
        var pos = _transform.GetWorldPosition(center);

        // Get all light entities within the specified radius
        _lightsInRange.Clear();
        _lookup.GetEntitiesInRange(Transform(center).Coordinates, range, _lightsInRange);
        foreach (var light in _lightsInRange)
        {
            if (lineOfSight) // If LoS is required, test it.
            {
                var lightPos = _transform.GetWorldPosition(light);
                var sqrDistance = Vector2.DistanceSquared(pos, lightPos);
                var ray = new CollisionRay(pos, (lightPos - pos).Normalized(), (int)CollisionGroup.Opaque);
                var hit = _physics.IntersectRay(_transform.GetMapId(center), ray, MathF.Sqrt(sqrDistance) - 0.5f, returnOnFirstHit: true);
                if (hit.Any() && hit.First().Distance != 0)
                    continue;
            }

            // If we reach here, the light is unobstructed and within range, break it.
            _light.TryDestroyBulb(light, light.Comp);
        }
    }

}
