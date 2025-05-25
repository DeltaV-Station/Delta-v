namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Raised on an upgrade after being installed in a gun.
/// </summary>
[ByRefEvent]
public readonly record struct GunUpgradeInstalledEvent(EntityUid Gun);
