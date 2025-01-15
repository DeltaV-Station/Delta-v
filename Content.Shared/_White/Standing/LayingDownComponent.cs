using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Standing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LayingDownComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float StandingUpTime { get; set; } = .5f;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float SpeedModify { get; set; } = .4f;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public bool AutoGetUp;
}
[Serializable, NetSerializable]
public sealed class ChangeLayingDownEvent : CancellableEntityEventArgs;

[Serializable, NetSerializable]
public sealed class CheckAutoGetUpEvent(NetEntity user) : CancellableEntityEventArgs
{
    public NetEntity User = user;
}
