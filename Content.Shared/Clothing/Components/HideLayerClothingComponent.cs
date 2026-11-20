using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// This is used for a clothing item that hides an appearance layer.
/// The entity's HumanoidAppearance component must have the corresponding hideLayerOnEquip value.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState] // DeltaV - network BlockLayerHide
public sealed partial class HideLayerClothingComponent : Component
{
    /// <summary>
    /// The appearance layer(s) to hide. Use <see cref='Layers'>Layers</see> instead.
    /// </summary>
    [DataField]
    [Obsolete("This attribute is deprecated, please use Layers instead.")]
    public HashSet<HumanoidVisualLayers>? Slots;

    /// <summary>
    /// A map of the appearance layer(s) to hide, and the equipment slot that should hide them.
    /// </summary>
    [DataField]
    public Dictionary<HumanoidVisualLayers, SlotFlags> Layers = new();

    /// <summary>
    /// If true, the layer will only hide when the item is in a toggled state (e.g. masks)
    /// </summary>
    [DataField]
    public bool HideOnToggle = false;

    /// <summary>
    /// DeltaV - If true, allow a player to right click and disable layer hiding
    /// </summary>
    [DataField]
    public bool AllowOverride = false;

    /// <summary>
    /// DeltaV - If false, the layer will not be hidden
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BlockLayerHide = false;
}
