using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Gives a gun upgrade a cost used with <see cref="UpgradeableGunCostComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeCostSystem))]
public sealed partial class GunUpgradeCostComponent : Component
{
    /// <summary>
    /// How much the upgrade costs
    /// </summary>
    [DataField]
    public int Cost = 30;
}
