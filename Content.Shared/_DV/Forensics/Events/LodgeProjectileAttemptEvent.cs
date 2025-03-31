using Content.Shared.Projectiles;

namespace Content.Shared._DV.Forensics.Events;

/// <summary>
/// Raised when a projectile strikes and damages a target, to allow the forensics
/// system to store the bullet for later retrieval.
/// </summary>
/// <param name="ProjUid">Entity for the projectile that has hit the target</param>
/// <param name="Component">Projectile component that has hit the target</param>
[ByRefEvent]
public record struct LodgeProjectileAttemptEvent(EntityUid ProjUid, ProjectileComponent Component);
