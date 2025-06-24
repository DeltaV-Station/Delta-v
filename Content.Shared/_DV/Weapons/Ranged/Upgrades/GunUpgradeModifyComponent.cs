using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Adds a comp registry to the fired projectiles.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeModifySystem))]
public sealed partial class GunUpgradeModifyComponent : Component
{
    /// <summary>
    /// The components to add to the projectile.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Added = new();
}
