using Content.Shared.FixedPoint;

namespace Content.Server._DV.Projectiles.Events;

/// <summary>
/// Raised when a piercing projectile that doesn't follow upstream piercing rules hits an entity.
/// </summary>
[ByRefEvent]
public record struct ProjectilePierceEvent(EntityUid Target, FixedPoint2 RequiredDamage, bool Pierced = false);
