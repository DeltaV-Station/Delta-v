using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// Indicates that the entity cannot be embedded with select projectiles.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EmbedImmuneComponent : Component
{
    /// <summary>
    /// A list of projectiles that this entity is immune to being embedded by.
    /// If null, the entity is immune to all projectiles.
    /// </summary>
    [DataField(required: true)]
    public HashSet<string> ImmuneTo = default!;
}
