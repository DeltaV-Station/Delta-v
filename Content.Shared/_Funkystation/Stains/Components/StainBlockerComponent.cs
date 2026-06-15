using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Stains.Components;

/// <summary>
/// Prevents entities equipped in specific slots underneath this item from getting stained
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StainBlockerComponent : Component
{
    [DataField("slots", required: true)]
    public SlotFlags BlockedSlots;
}
