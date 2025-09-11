using Content.Shared.Projectiles;
using Content.Shared.Whitelist;

namespace Content.Shared._DV.Projectiles;

public sealed partial class SharedEmbedImmuneSystem : EntitySystem
{

    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

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
        if (_whitelist.IsWhitelistFail(entity.Comp.ImmuneTo, args.ProjUid))
            return;

        args.Cancelled = true;
    }
}
