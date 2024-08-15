using Robust.Shared.GameStates;
using Robust.Shared.Serialization;


namespace Content.Shared.Stray.Plesen.PlesenWall;

//[Serializable, NetSerializable]
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
[Access(typeof(SharedPlesenWallSystem))]
public sealed partial class PlesenWallComponent : Component
{
    [DataField("health"),ViewVariables(VVAccess.ReadWrite)]
    public float health = 100;

    [ViewVariables(VVAccess.ReadOnly)]
    public float realHealth = 100;
    [ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public bool fullyGroth = false;
    //[ViewVariables(VVAccess.ReadWrite)]
    //public bool prefullyGroth = false;
    //[ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan growAfter = TimeSpan.Zero;
}
