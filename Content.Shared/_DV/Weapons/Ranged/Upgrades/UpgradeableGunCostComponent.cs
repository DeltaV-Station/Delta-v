using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Makes an upgradeable gun limit upgrades based on their <see cref="GunUpgradeCostComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeCostSystem))]
[AutoGenerateComponentState]
public sealed partial class UpgradeableGunCostComponent : Component
{
    /// <summary>
    /// Max cost upgrades can add up to.
    /// If adding an upgrade would exceed this limit, it cannot be installed.
    /// </summary>
    [DataField]
    public int MaxCost = 100;

    /// <summary>
    /// How much cost is currently used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int UsedCost;
}
