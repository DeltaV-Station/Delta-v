using Content.Shared._DV.Clothing.Systems;
using Robust.Shared.GameStates;

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
    /// The container name which holds the lanyard label
    /// </summary>
    [DataField]
    public string SlotName = "lanyard_label";
}
