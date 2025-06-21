using Content.Shared.Projectiles;

namespace Content.Shared._DV.Projectiles;

public abstract class SharedPressureProjectileSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PressureProjectileComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(Entity<PressureProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        float pressure = GetPressure(ent);
        if (pressure < ent.Comp.MinPressure || pressure > ent.Comp.MaxPressure)
            args.Damage *= ent.Comp.Modifier;
    }

    /// <summary>
    /// Multiply the modifier by some factor.
    /// </summary>
    public void MultiplyModifier(Entity<PressureProjectileComponent?> ent, float factor)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.Modifier *= factor;
        // no dirty since shooting is almost guaranteed to happen in pvs range, mispredict is nearly impossible
        // (and projectile hit event isnt predicted anyway)
    }

    // client assumes it to always be at minimum allowed pressure to correctly predict for the intended use case of lavaland.
    // it also means it would mispredict targets staying alive rather than falsely falling over which is far more annoying.
    protected virtual float GetPressure(Entity<PressureProjectileComponent> ent) => ent.Comp.MinPressure;
}
