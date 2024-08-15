using Robust.Shared.GameStates;
using Robust.Shared.Serialization;


namespace Content.Shared.Stray.Plesen.PlesenFloor;

//[Serializable, NetSerializable]
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
[Access(typeof(SharedPlesenFloorSystem))]
public sealed partial class PlesenFloorComponent : Component
{
    [DataField("health"),ViewVariables(VVAccess.ReadWrite)]
    public float health = 50;

    [ViewVariables(VVAccess.ReadOnly)]
    public float realHealth = 50;
    [ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public bool fullyGroth = false;
    //[ViewVariables(VVAccess.ReadWrite)]
    //public bool prefullyGroth = false;
    //[ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan growAfter = TimeSpan.Zero;
}
