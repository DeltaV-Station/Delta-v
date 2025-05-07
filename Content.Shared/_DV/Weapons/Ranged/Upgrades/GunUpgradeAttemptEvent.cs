namespace Content.Shared._DV.Weapons.Ranged.Upgrades;

/// <summary>
/// Raised on a gun upgrade to let it cancel upgrading.
/// </summary>
[ByRefEvent]
public record struct GunUpgradeAttemptEvent(EntityUid Gun, EntityUid User, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}
