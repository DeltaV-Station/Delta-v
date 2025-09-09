using Content.Shared.Projectiles;

namespace Content.Shared._DV.Projectiles;

public sealed partial class SharedEmbedImmuneSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmbedImmuneComponent, ProjectileReflectAttemptEvent>(OnProjectileReflectAttempt);
    }

    private void OnProjectileReflectAttempt(Entity<EmbedImmuneComponent> entity, ref ProjectileReflectAttemptEvent args)
    {
        if (!TryComp<EmbeddableProjectileComponent>(args.ProjUid, out var embeddable))
            return;

        // If we're blacklisted, bounce the projectile.
        if (!entity.Comp.ImmuneTo.Contains(Prototype(args.ProjUid)?.ID ?? ""))
            return;

        args.Cancelled = true;
    }
}
