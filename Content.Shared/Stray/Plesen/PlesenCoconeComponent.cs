using Robust.Shared.GameStates;
using Robust.Shared.Serialization;


namespace Content.Shared.Stray.Plesen.PlesenCocone;

//[Serializable, NetSerializable]
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedPlesenCoconeSystem))]
public sealed partial class PlesenCoconeComponent : Component
{
    [DataField("health"),ViewVariables(VVAccess.ReadWrite)]
    public float health = 50;
    [ViewVariables(VVAccess.ReadOnly)]
    public float realHealth = 100;
}
