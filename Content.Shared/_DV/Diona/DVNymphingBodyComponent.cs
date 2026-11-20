using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

[RegisterComponent, NetworkedComponent]
public sealed partial class DVNymphingBodyComponent : Component
{
    /// <summary>
    /// The text that appears when attempting to split.
    /// </summary>
    [DataField]
    public LocId PopupText = "diona-gib-action-use";
}
