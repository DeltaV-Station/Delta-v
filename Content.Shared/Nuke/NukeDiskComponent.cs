using Robust.Shared.GameStates;

namespace Content.Shared.Nuke;

/// <summary>
/// Used for tracking the nuke disk - isn't a tag for pinpointer purposes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NukeDiskComponent : Component
{
    /// <summary>
    /// DeltaV: When extracted by a syndie, this makes the disk teleport to any nukies.
    /// </summary>
    [DataField]
    public bool Extracted;
}
