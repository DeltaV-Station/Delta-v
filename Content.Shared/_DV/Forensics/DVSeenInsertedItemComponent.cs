using Robust.Shared.GameStates;

namespace Content.Shared._DV.Forensics;

[RegisterComponent, NetworkedComponent]
public sealed partial class DVSeenInsertedItemComponent : Component
{
    [DataField(required: true)]
    public string ItemSlot;
}
