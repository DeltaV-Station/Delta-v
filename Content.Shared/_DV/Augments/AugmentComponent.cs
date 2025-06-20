using Robust.Shared.GameStates;

namespace Content.Shared._DV.Augments;

/// <summary>
///     Component that indicates an entity is an augment
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentComponent : Component;

/// <summary>
///     Component that tracks which augments are installed on this body
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InstalledAugmentsComponent : Component
{
    [DataField]
    public HashSet<NetEntity> InstalledAugments = new();
}
