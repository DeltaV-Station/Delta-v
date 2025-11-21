using Content.Shared.FixedPoint;

namespace Content.Server._DV.Projectiles.Events;

/// <summary>
/// Raised when a piercing projectile hits an entity that doesn't follow upstream piercing rules.
/// </summary>
[ByRefEvent]
public record struct ProjectilePierceEvent(EntityUid Target, FixedPoint2 RequiredDamage, bool Pierced = false);
