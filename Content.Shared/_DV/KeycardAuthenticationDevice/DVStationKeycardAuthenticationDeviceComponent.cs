using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.KeycardAuthenticationDevice;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedDVStationKeycardAuthenticationDeviceSystem))]
public sealed partial class DVStationKeycardAuthenticationDeviceComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan AccessibleAfter = TimeSpan.Zero;

    [DataField]
    public TimeSpan FailureDelay = TimeSpan.FromMinutes(2);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? SwipesStartedAt = null;

    [DataField]
    public TimeSpan SwipeWindow = TimeSpan.FromSeconds(10);

    [DataField(required: true)]
    public Dictionary<DVStationKeycardAction, int> ActionThresholds;

    [DataField, AutoNetworkedField]
    public int Swipes;

    [DataField, AutoNetworkedField]
    public DVStationKeycardAction? SwipingFor;

    [DataField]
    public LocId FailureAnnouncementSender = "keycard-authentication-device-sender";

    [DataField]
    public LocId FailureAnnouncement = "keycard-authentication-device-warning";

    [DataField]
    public Color FailureAnnouncementColor = Color.FromHex("#e93a9a");

    [DataField]
    public TimeSpan FailureElectrocutionDuration = TimeSpan.FromSeconds(5);
}

[Serializable, NetSerializable]
public enum DVStationKeycardAction
{
    Mayday,
    Scuttling,
}
