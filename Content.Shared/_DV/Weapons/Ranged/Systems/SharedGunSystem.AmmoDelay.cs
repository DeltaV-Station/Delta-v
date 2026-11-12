using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    public void InitializeAmmoDelay()
    {
    }
    public void AmmoDelay(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (GetChamberEntity(ent) is not { } ammo)
            return;

        if (!TryComp(ammo, out AmmoDelayComponent? ammoDelay))
            return;

        if (ammoDelay.Delay is { })
        {
            ent.Comp.Delay = ammoDelay.Delay;
            return;
        }
        ent.Comp.MinimumDelay = ammoDelay.MinimumDelay;
        ent.Comp.MaximumDelay = ammoDelay.MaximumDelay;
        Dirty(args.Used.Owner, ent.Comp);
    }
}
