using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// While this entity is ActivelyEmbedded, it will run the given Effects on its containing entity
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ForeignBodyEffectsComponent : Component
{
    [DataField]
    public bool WorksOnTheDead;

    [DataField("effects", required: true, serverOnly: true)]
    public EntityEffect[] Effects = [];

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
}

/// <summary>
/// Marker component for when a foreign body is actively embedded
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ForeignBodyActivelyEmbeddedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ActiveAfter;
}

/// <summary>
/// Raised on an entity when an internally embedded projectile attempts to run effects on it
/// </summary>
[ByRefEvent]
public readonly record struct ForeignBodyEffectsEvent(Entity<ForeignBodyEffectsComponent> Embedded);
