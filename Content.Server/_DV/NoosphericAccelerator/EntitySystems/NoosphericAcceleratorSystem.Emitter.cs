using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Shared.Projectiles;
using Content.Shared._DV.NoosphericAccelerator.Components;
using Robust.Shared.Physics.Components;
using Content.Server._DV.Singularity.Components;
using Content.Shared._DV.Noospherics;

namespace Content.Server._DV.NoosphericAccelerator.EntitySystems;

public sealed partial class NoosphericAcceleratorSystem
{
    private void FireEmitter(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent comp,
        NoosphericAcceleratorEmitterComponent? emitter = null)
    {
        if (!Resolve(uid, ref emitter))
            return;

        var xformQuery = GetEntityQuery<TransformComponent>();
        if (!xformQuery.TryGetComponent(uid, out var xform))
        {
            Log.Error(
                "NoosphericAccelerator attempted to emit a particle without (having) a transform from which to base its initial position and orientation.");
            return;
        }

        var emitted = Spawn(emitter.EmittedPrototype, xform.Coordinates);

        if (xformQuery.TryGetComponent(emitted, out var particleXform))
            _transformSystem.SetLocalRotation(emitted, xform.LocalRotation, particleXform);

        if (TryComp<PhysicsComponent>(emitted, out var particlePhys))
        {
            var angle = _transformSystem.GetWorldRotation(uid, xformQuery);
            _physicsSystem.SetBodyStatus(emitted, particlePhys, BodyStatus.InAir);

            var velocity = angle.ToWorldVec() * 20f;
            if (TryComp<PhysicsComponent>(uid, out var phys))
                velocity += phys
                    .LinearVelocity; // Inherit velocity from parent so if the clown has strapped a dozen engines to departures we don't outpace the particles.

            _physicsSystem.SetLinearVelocity(emitted, velocity, body: particlePhys);
        }

        if (TryComp<ProjectileComponent>(emitted, out var proj))
            _projectileSystem.SetShooter(emitted, proj, uid);

        if (TryComp<NoosphericFoodComponent>(emitted, out var food))
        {
            var strength = comp.SelectedStrength;

            foreach (var type in Enum.GetValues<ParticleType>())
            {
                food.Particles[type] = strength.Particles[type] switch
                {
                    NoosphericAcceleratorPowerLevel.Standby => comp.PowerMappings[0],
                    NoosphericAcceleratorPowerLevel.Level0 => comp.PowerMappings[1],
                    NoosphericAcceleratorPowerLevel.Level1 => comp.PowerMappings[2],
                    NoosphericAcceleratorPowerLevel.Level2 => comp.PowerMappings[3],
                    NoosphericAcceleratorPowerLevel.Level3 => comp.PowerMappings[4],
                    _ => 0,
                } * comp.PowerModifier;
            }
        }

        if (TryComp<NoosphericeProjectileComponent>(emitted, out var particle))
            particle.State = comp.SelectedStrength;

        // DONOTMERGE-TODO: Re-enable this
        //_appearanceSystem.SetData(emitted, NoosphericAcceleratorVisuals.VisualState, comp.SelectedStrength);
    }
}
