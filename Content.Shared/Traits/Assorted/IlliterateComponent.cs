using Content.Shared.Paper;
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// This is used for the illiterate trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IlliterateComponent : Component
{
    /// <summary>
    /// What message should be displayed when the character fails to write?
    /// </summary>
    public LocId FailWriteMessage = "paper-component-illiterate";
}
