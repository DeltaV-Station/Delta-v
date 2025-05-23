using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Spawners;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

public sealed class GunUpgradeRangeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunUpgradeRangeComponent, GunShotEvent>(OnGunShot);
    }

    private void OnGunShot(Entity<GunUpgradeRangeComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (ammo is not {} uid)
                continue;

            Comp<TimedDespawnComponent>(uid).Lifetime *= ent.Comp.Coefficient;
        }
    }
}
