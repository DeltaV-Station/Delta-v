using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Upgrades.Components;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

public sealed class GunUpgradeCostSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunUpgradeCostComponent, ExaminedEvent>(OnUpgradeExamined);
        SubscribeLocalEvent<GunUpgradeCostComponent, GunUpgradeAttemptEvent>(OnUpgradeAttempt);
        SubscribeLocalEvent<GunUpgradeCostComponent, GunUpgradeInstalledEvent>(OnUpgradeInstalled);

        SubscribeLocalEvent<UpgradeableGunCostComponent, ExaminedEvent>(OnGunExamined);
        SubscribeLocalEvent<UpgradeableGunCostComponent, EntRemovedFromContainerMessage>(OnGunRemoved);
    }

    private void OnUpgradeExamined(Entity<GunUpgradeCostComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        // the popup assumes the gun MaxCost is 100 :(
        args.PushMarkup(Loc.GetString("gun-upgrade-cost-examine", ("cost", ent.Comp.Cost)));
    }

    private void OnUpgradeAttempt(Entity<GunUpgradeCostComponent> ent, ref GunUpgradeAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var gun = args.Gun;
        if (!TryComp<UpgradeableGunCostComponent>(gun, out var comp))
            return;

        if (comp.UsedCost + ent.Comp.Cost <= comp.MaxCost)
            return;

        _popup.PopupClient(Loc.GetString("upgradeable-gun-popup-insufficient-cost"), gun, args.User);
        args.Cancelled = true;
    }

    private void OnUpgradeInstalled(Entity<GunUpgradeCostComponent> ent, ref GunUpgradeInstalledEvent args)
    {
        var gun = args.Gun;
        // shouldnt change between last event
        var comp = Comp<UpgradeableGunCostComponent>(gun);

        comp.UsedCost += ent.Comp.Cost;
        Dirty(gun, comp);
    }

    private void OnGunExamined(Entity<UpgradeableGunCostComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        // if you make a gun that has non-100 MaxCost change this to actual % calculation :)
        var remaining = ent.Comp.MaxCost - ent.Comp.UsedCost;
        args.PushMarkup(Loc.GetString("upgradeable-gun-cost-examine", ("remaining", remaining)));
    }

    private void OnGunRemoved(Entity<UpgradeableGunCostComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<UpgradeableGunComponent>(ent, out var upgradeable))
            return;

        if (args.Container.ID != upgradeable.UpgradesContainerId)
            return;

        if (!TryComp<GunUpgradeCostComponent>(args.Entity, out var upgrade))
            return;

        ent.Comp.UsedCost -= upgrade.Cost;
        Dirty(ent);
    }
}
