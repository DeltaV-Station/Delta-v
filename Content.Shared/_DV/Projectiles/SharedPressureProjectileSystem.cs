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

    // client assumes it to be in a vacuum to correctly predict for the intended use case of lavaland/space.
    // it also means it would mispredict targets staying alive rather than falsely falling over which is far more annoying.
    protected virtual float GetPressure(EntityUid uid) => 0f;
}
