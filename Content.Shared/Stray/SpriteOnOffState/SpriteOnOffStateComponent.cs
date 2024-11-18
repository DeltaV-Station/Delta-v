using Robust.Shared.GameStates;

namespace Content.Shared.Stray.SpriteOnOffState;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedSpriteOnOffStateSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class SpriteOnOffStateComponent : Component
{

    [AutoNetworkedField, DataField("IsOn", required: true),ViewVariables(VVAccess.ReadWrite)]
    public bool IsOn = true;
    [DataField("OnState", required: true),ViewVariables(VVAccess.ReadWrite)]
    public string OnState;
    [DataField("OffState", required: true),ViewVariables(VVAccess.ReadWrite)]
    public string OffState;
    [DataField("Popup"),ViewVariables(VVAccess.ReadWrite)]
    public string Popup = "";
}
