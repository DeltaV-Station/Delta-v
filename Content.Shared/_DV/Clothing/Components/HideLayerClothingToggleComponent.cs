using Robust.Shared.GameStates;

namespace Content.Shared._DV.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HideLayerClothingToggleComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool HideLayers = true;
}
