using Content.Shared._DV.Clothing.Systems;
using Robust.Shared.GameStates;
using Content.Shared.Containers.ItemSlots;

namespace Content.Shared._DV.Clothing.Components;

/// <summary>
/// This component allows you to place a piece of paper into a clothing item
/// and examine this text in a different tab in the examine UI
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(LanyardSystem))]
public sealed partial class LanyardComponent : Component
{
    /// <summary>
    /// The slot which stores the lanyard label
    /// </summary>
    [DataField]
    public ItemSlot LabelSlot = new();
}
