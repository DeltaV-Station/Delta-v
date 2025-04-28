using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// When a projectile attempts to internally embed into this entity, redirect it to the body parts
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BodyForeignBodyContainerComponent : Component;

/// <summary>
/// Container for body parts for internally embedded projectiles
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BodyPartForeignBodyContainerComponent : Component
{
    [DataField]
    public string ContainerName = "internally_embedded_projectiles";
}
