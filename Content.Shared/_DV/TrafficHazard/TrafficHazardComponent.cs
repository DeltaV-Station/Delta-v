using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.TrafficHazard;

/// <summary>
/// If this object moving at high speeds is a risk to others.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TrafficHazardComponent : Component
{
    /// <summary>
    /// The minimum relative speed between this entity and the collided entity to count as a hit.
    /// </summary>
    [DataField]
    public float MinimumSpeedDifference = 8.0f;

    /// <summary>
    /// The minimum speed this entity needs to be going relative to its parent to count as a collision (So hitting a stationary object doesn't run you over)
    /// </summary>
    [DataField]
    public float MinimumSpeed = 5.0f;

    /// <summary>
    /// The amount of damage to deal on a successful collision.
    /// If null, does not do any damage on a collision.
    /// </summary>
    [DataField]
    public DamageSpecifier? CollisionDamage = default!;

    /// <summary>
    /// How long to stun/knock the collided target.
    /// </summary>
    [DataField]
    public float StunTime = 2f;

    /// <summary>
    /// Should this transfer velocity on a collision?
    /// </summary>
    public bool Bonk = true;

    /// <summary>
    /// Collision noise when hitting someone.
    /// </summary>
    [DataField]
    public SoundSpecifier? CollisionSound = new SoundCollectionSpecifier("GenericHit");

    /// <summary>
    /// Used to prevent re-firing for hazard-on-hazard collision.
    /// </summary>
    [DataField]
    public TimeSpan AvoidRefire = TimeSpan.MinValue;

}
