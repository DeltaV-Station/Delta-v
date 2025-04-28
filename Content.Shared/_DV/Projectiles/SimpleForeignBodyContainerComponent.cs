using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// When a projectile attempts to embed into this entity, store it on this entity itself
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SimpleForeignBodyContainerComponent : Component
{
    [DataField]
    public string ContainerName = "internally_embedded_projectiles";
}
