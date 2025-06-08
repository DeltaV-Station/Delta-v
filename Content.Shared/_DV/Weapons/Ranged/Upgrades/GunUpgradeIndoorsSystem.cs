using Content.Shared._DV.Projectiles;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Upgrades;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

public sealed class GunUpgradeIndoorsSystem : EntitySystem
{
    [Dependency] private readonly GunUpgradeSystem _upgrade = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPressureProjectileSystem _pressure = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunUpgradeIndoorsComponent, GunUpgradeAttemptEvent>(OnUpgradeAttempt);
        SubscribeLocalEvent<GunUpgradeIndoorsComponent, GunShotEvent>(OnGunShot);
    }

    private void OnUpgradeAttempt(Entity<GunUpgradeIndoorsComponent> ent, ref GunUpgradeAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var count = 0;
        foreach (var upgrade in _upgrade.GetCurrentUpgrades(args.Gun))
        {
            // only allow 2 of them to be installed max to prevent doing double damage
            if (!HasComp<GunUpgradeIndoorsComponent>(ent) || ++count < 2)
                continue;

            _popup.PopupClient(Loc.GetString("upgradeable-gun-popup-too-many"), args.Gun, args.User);
            args.Cancelled = true;
            return;
        }
    }

    private void OnGunShot(Entity<GunUpgradeIndoorsComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (ammo is not {} uid)
                continue;

            _pressure.MultiplyModifier(uid, ent.Comp.Multiplier);
        }
    }
}
