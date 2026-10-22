using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Projectiles.Components;

[RegisterComponent]
public sealed partial class PiercingProjectileComponent : Component
{
    /// <summary>
    /// The health threshold that a target has to supersede to block the projectile.
    /// A normal wall has 200.
    /// </summary>
    [DataField(required: true)]
    public float HealthThreshold;

    /// <summary>
    /// The number of entities it pierced.
    /// It'll only count the entities with the <see cref="PierceBlockTag"/>.
    /// </summary>
    [DataField]
    public float PierceCounter;

    /// <summary>
    /// The whitelist for checking what increments the <see cref="PierceCounter"/>.
    /// </summary>
    /// <example>
    /// If this has the tag "Wall" in it, any entity with the tag "Wall" will increment <see cref="PierceCounter"/>
    /// upon being hit.
    /// </example>
    [DataField]
    public EntityWhitelist PierceCounterWhitelist;

    /// <summary>
    /// The number of entities it is allowed to pierce before being deleted.
    /// When <see cref="PierceCounter"/> is higher than this number, it'll be deleted.
    /// </summary>
    /// <example>
    /// A bullet with a MaxPierceNumberThreshold of 3 will pierce 3 entities with the <see cref="PierceBlockTag"/> and be deleted when hitting the fourth.
    /// </example>
    [DataField]
    public float MaxPierceNumberThreshold = 1f;

    /// <summary>
    /// The cardinal direction that the bullet is flying toward.
    /// </summary>
    [DataField]
    public Direction? Direction;

    /// <summary>
    /// The vertical/horizontal coordinate that will be ignored for <see cref="PierceCounter"/>.
    /// </summary>
    /// <example>
    /// A bullet going positive Y hitting a wall at X 3 and Y 5 will not increment the <see cref="PierceCounter"/> when hitting other walls at Y 5.
    /// </example>
    [DataField]
    public float? IgnoreRowCoordinate;
}
