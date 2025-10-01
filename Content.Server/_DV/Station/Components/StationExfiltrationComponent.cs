using Content.Server._DV.Station.Systems;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Server._DV.Station.Components;

[RegisterComponent, AutoGenerateComponentPause, Access(typeof(StationExfiltrationSystem))]
public sealed partial class StationExfiltrationComponent : Component
{
    /// <summary>
    /// Time at which the shuttle will dock to the station
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? ArrivalTime;

    /// <summary>
    /// Time at which the shuttle will announce that it's leaving the station
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? ImpendingDepartureAnnouncementTime;

    /// <summary>
    /// Time at which the shuttle leave the station
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? DepartureTime;

    /// <summary>
    /// How long it takes for the shuttle to arrive at the station
    /// </summary>
    [DataField]
    public TimeSpan TravelTime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// How long before leaving to warn
    /// </summary>
    [DataField]
    public TimeSpan DepartureWarningTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long to wait at the station before leaving
    /// </summary>
    [DataField]
    public TimeSpan LeaveTime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// The dock tag the shuttle will prefer to dock to
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> DockTo = "DockArrivals";

    /// <summary>
    /// The map to spawn for the shuttle
    /// </summary>
    [DataField]
    public ResPath ShuttlePath = new("/Maps/_DV/Shuttles/exfiltration.yml");

    /// <summary>
    /// The spawned shuttle
    /// </summary>
    [DataField]
    public EntityUid? SpawnedShuttle;

    /// <summary>
    /// Sender for exfiltration announcements
    /// </summary>
    [DataField]
    public LocId Sender = "exfiltration-shuttle-sender";

    /// <summary>
    /// The announcement for when an exfiltration shuttle is called
    /// </summary>
    [DataField]
    public LocId CalledAnnouncement = "exfiltration-shuttle-called";

    /// <summary>
    /// The announcement for when an exfiltration shuttle is recalled
    /// </summary>
    [DataField]
    public LocId RecalledAnnouncement = "exfiltration-shuttle-recalled";

    /// <summary>
    /// The announcement for when the exfiltration shuttle docks in the vicinity of the station
    /// </summary>
    [DataField]
    public LocId DockedNearbyStationAnnouncement = "exfiltration-shuttle-docked-nearby-station";

    /// <summary>
    /// The announcement for when the exfiltration shuttle docks at a station port
    /// </summary>
    [DataField]
    public LocId DockedAtStationAnnouncement = "exfiltration-shuttle-docked-at-station";

    /// <summary>
    /// The announcement for when the exfiltration shuttle is about to leave
    /// </summary>
    [DataField]
    public LocId AboutToLeaveAnnouncement = "exfiltration-shuttle-about-to-leave";

    /// <summary>
    /// The announcement for when the exfiltration shuttle leaves
    /// </summary>
    [DataField]
    public LocId LeftAnnouncement = "exfiltration-shuttle-left";

    /// <summary>
    /// The announcement for when the exfiltration cannot be called due to technical issues
    /// </summary>
    [DataField]
    public LocId FailedAnnouncement = "exfiltration-shuttle-failed";
}
