using Content.Shared.Weapons.Ranged.Upgrades.Components;

namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Raised on a gun upgrade to let it cancel upgrading.
/// </summary>
[ByRefEvent]
public record struct GunUpgradeAttemptEvent(Entity<UpgradeableGunComponent> Gun, EntityUid User, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}
