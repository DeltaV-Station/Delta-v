using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    public void AmmoDelay(Entity<GunDelayComponent> ent, ref ShotAttemptedEvent args)
    {
        if (GetChamberEntity(ent) is not { } ammo)
            return;

        if (!TryComp(ammo, out AmmoDelayComponent? ammoDelay))
            return;

        if (ammoDelay.Delay is not null)
        {
            ent.Comp.Delay = ammoDelay.Delay;
            Dirty(ent);
            return;
        }
        ent.Comp.MinimumDelay = ammoDelay.MinimumDelay;
        ent.Comp.MaximumDelay = ammoDelay.MaximumDelay;
        Dirty(ent);
    }
}
