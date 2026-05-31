using Robust.Shared.GameStates;

namespace Content.Shared._DV.KeycardAuthenticationDevice;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(DVStationKeycardAuthenticationDeviceSystem))]
public sealed partial class DVStationKeycardAuthenticationDeviceComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan AccessibleAfter = TimeSpan.Zero;

    [DataField]
    public TimeSpan FailureDelay = TimeSpan.FromMinutes(2);

    [DataField]
    public Dictionary<DVStationKeycardAction, int> ActionThresholds;

    [DataField]
    public int Swipes = 0;
}

[Serializable, NetSerializable]
public enum DVStationKeycardAction
{
    Mayday,
    Scuttling,
}
