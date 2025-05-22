using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

public sealed class GunUpgradeModifySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunUpgradeModifyComponent, GunShotEvent>(OnGunShot);
    }

    private void OnGunShot(Entity<GunUpgradeModifyComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (ammo is {} uid)
                EntityManager.AddComponents(uid, ent.Comp.Added);
        }
    }
}
