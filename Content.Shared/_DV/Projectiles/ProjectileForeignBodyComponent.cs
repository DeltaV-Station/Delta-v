using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// When this projectile hits a target, it will spawn the ForeignBody inside of them
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileForeignBodyComponent : Component
{
    /// <summary>
    /// The foreign body to spawn inside the target
    /// </summary>
    [DataField(required: true)]
    public EntProtoId ForeignBody;

    /// <summary>
    /// The base chance of the foreign body to spawn inside the target
    /// </summary>
    [DataField]
    public float BaseChance = 0.5f;

    /// <summary>
    /// The damage type to check for armour modification
    /// </summary>
    [DataField]
    public ProtoId<DamageTypePrototype> DamageType = "Piercing";

    /// <summary>
    /// When to begin effects
    /// </summary>
    [DataField]
    public TimeSpan EffectsBeginAfter = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Raised directed on an entity when an internally embeddable projectile attempts to embed into it
/// </summary>
[ByRefEvent]
public readonly record struct ProjectileForeignBodyAttemptEvent(EntityUid Shooter, EntityUid Weapon, EntityUid EmbeddedIn, Entity<ProjectileForeignBodyComponent> Embedded);
