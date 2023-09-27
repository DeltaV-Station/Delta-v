using Robust.Shared.Containers;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Soul;

public abstract class SharedGolemSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // I can think of better ways to handle this, but they require API changes upstream.
        SubscribeLocalEvent<GunHeldByGolemComponent, AttemptShootEvent>(OnAttemptShoot);
    }

    protected void SharedOnEntInserted(EntInsertedIntoContainerMessage args)
    {
        if (HasComp<GunComponent>(args.Entity))
            AddComp<GunHeldByGolemComponent>(args.Entity);
    }

    protected void SharedOnEntRemoved(EntRemovedFromContainerMessage args)
    {
        if (HasComp<GunComponent>(args.Entity))
            RemComp<GunHeldByGolemComponent>(args.Entity);
    }

    private void OnAttemptShoot(EntityUid uid, GunHeldByGolemComponent component, ref AttemptShootEvent args)
    {
        args.Cancelled = true;
        args.Message = Loc.GetString("golem-no-using-guns-popup");
    }
}
