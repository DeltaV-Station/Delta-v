using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Modifies the <c>TimedDespawn</c> lifetime to change its range while keeping its speed the same.
/// Requires that the projectile entity has <c>TimedDespawnComponent</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeRangeSystem))]
public sealed partial class GunUpgradeRangeComponent : Component
{
    [DataField(required: true)]
    public float Coefficient = 1f;
}
