using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Multiplies <c>PressureProjectileComponent.Modifier</c> of the projectile by a value.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeIndoorsSystem))]
public sealed partial class GunUpgradeIndoorsComponent : Component
{
    /// <summary>
    /// Pressure modifier is multiplied by this by each upgrade.
    /// </summary>
    [DataField]
    public float Multiplier = 2f;
}
