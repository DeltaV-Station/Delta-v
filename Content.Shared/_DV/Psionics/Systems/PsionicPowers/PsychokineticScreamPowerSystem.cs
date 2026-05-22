using System.Linq;
using System.Numerics;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Coordinates;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Physics;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This system enables a psionic being to break lights around them.
/// </summary>
public sealed class PsychokineticScreamPowerSystem : BasePsionicPowerSystem<PsychokineticScreamPowerComponent, PsychokineticScreamPowerActionEvent>
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPoweredLightSystem _light = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    protected override void OnPowerUsed(Entity<PsychokineticScreamPowerComponent> psionic, ref PsychokineticScreamPowerActionEvent args)
    {
        if (psionic.Comp.AbilitySound != null)
            _audio.PlayPredicted(psionic.Comp.AbilitySound, psionic, psionic);

        ShatterLightsAround(psionic.Owner, psionic.Comp.Radius, psionic.Comp.LineOfSight, psionic.Comp.PenetratingRadius);

        SpawnAttachedTo(psionic.Comp.Effect, psionic.Owner.ToCoordinates());
        args.Handled = true;
    }

    /// <summary>
    /// Shatter lights around an entity.
    /// This is public because other systems require it.
    /// </summary>
    /// <param name="source">The entity that caused it.</param>
    /// <param name="range">The range where the lights are broken within.</param>
    /// <param name="lineOfSight">Whether line of sight is required.</param>
    /// <param name="penetratingRadius">How far it ignores the line of sight.</param>
    [PublicAPI]
    public void ShatterLightsAround(EntityUid source, float range, bool lineOfSight, float penetratingRadius = 0f)
    {
        var pos = _transform.GetWorldPosition(source);

        // Get all light entities within the specified radius
        HashSet<Entity<PoweredLightComponent>> lightsInRange = [];
        _lookup.GetEntitiesInRange(Transform(source).Coordinates, range, lightsInRange);

        foreach (var light in lightsInRange)
        {
            if (lineOfSight) // If LoS is required, test it.
            {
                var lightPos = _transform.GetWorldPosition(light);
                var sqrDistance = Vector2.DistanceSquared(pos, lightPos);
                if (sqrDistance > penetratingRadius * penetratingRadius)
                {
                    // If the light is outside the penetrating radius, do a LoS check.
                    var ray = new CollisionRay(pos, (lightPos - pos).Normalized(), (int)CollisionGroup.Opaque);
                    var hit = _physics.IntersectRay(_transform.GetMapId(source), ray, MathF.Sqrt(sqrDistance) - 0.5f, returnOnFirstHit: true);
                    if (hit.Any() && hit.First().Distance != 0)
                        continue;
                }
            }

            // If we reach here, the light is unobstructed and within range, break it.
            _light.TryDestroyBulb(light, light.Comp, source);
        }
    }
}
