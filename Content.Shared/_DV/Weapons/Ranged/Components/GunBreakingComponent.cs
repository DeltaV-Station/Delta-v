using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GunBreakingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Broken = false;
}
