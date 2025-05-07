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
        if (GetPressure(ent) > ent.Comp.MaxPressure)
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

    // client assumes it to be in a vacuum to correctly predict for the intended use case of lavaland/space.
    // it also means it would mispredict targets staying alive rather than falsely falling over which is far more annoying.
    protected virtual float GetPressure(EntityUid uid) => 0f;
}
