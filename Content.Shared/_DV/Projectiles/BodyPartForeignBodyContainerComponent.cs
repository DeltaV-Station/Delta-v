using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// Container for body parts for internally embedded projectiles
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BodyPartForeignBodyContainerComponent : Component
{
    [DataField]
    public string ContainerName = "internally_embedded_projectiles";

    public Container Container = default!;
}
