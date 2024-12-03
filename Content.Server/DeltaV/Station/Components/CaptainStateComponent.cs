using Content.Server.DeltaV.Station.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.DeltaV.Station.Components;

/// <summary>
/// Denotes a station has no captain and holds data for automatic ACO systems
/// </summary>
[RegisterComponent, Access(typeof(CaptainStateSystem), typeof(StationSystem)), AutoGenerateComponentPause]
public sealed partial class CaptainStateComponent : Component
{
    /// <summary>
    /// Denotes wether the entity has a captain or not
    /// </summary>
    /// <remarks>
    /// Assume no captain unless specified
    /// </remarks>
    [DataField]
    public bool HasCaptain;

    /// <summary>
    /// Holds the round time of the last time a captain was present if one is not present currently
    /// </summary>
    [DataField("TimeSinceCaptain", customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan TimeSinceCaptain = TimeSpan.Zero;

    /// <summary>
    /// How long with no captain before an ACO is requested
    /// </summary>
    [DataField]
    public TimeSpan ACORequestDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    /// The localization ID used for announcing the cancellation of ACO requests
    /// </summary>
    [DataField]
    public LocId RevokeACOMessage = "captain-arrived-revoke-aco-announcement";

    /// <summary>
    /// The localization ID for requesting an ACO vote when AA will be unlocked
    /// </summary>
    [DataField]
    public LocId ACORequestWithAAMessage = "no-captain-request-aco-vote-with-aa-announcement";

    /// <summary>
    /// The localization ID for requesting an ACO vote when AA will not be unlocked
    /// </summary>
    [DataField]
    public LocId ACORequestNoAAMessage = "no-captain-request-aco-vote-announcement";

    /// <summary>
    /// Set after ACO has been requested to avoid duplicate calls
    /// </summary>
    [DataField]
    public bool IsACORequestActive;

    /// <summary>
    /// Used to denote that AA should be unlocked after the delay
    /// </summary>
    [DataField]
    public bool UnlockAA = true;

    /// <summary>
    /// How long after ACO is requested the spare id cabinet will be unlocked if applicable
    /// </summary>
    [DataField]
    public TimeSpan UnlockAADelay = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The localization ID for announcing that AA has been unlocked for ACO
    /// </summary>
    [DataField]
    public LocId AAUnlockedMessage = "no-captain-aa-unlocked-announcement";

    /// <summary>
    /// The access level used to identify spare ID cabinets
    /// </summary>
    [DataField]
    public List<ProtoId<AccessLevelPrototype>> EmergencyAAAccess = new() { "DV-SpareSafe" };

    /// <summary>
    /// The access level to grant to spare ID cabinets
    /// </summary>
    [DataField]
    public List<ProtoId<AccessLevelPrototype>> ACOAccess = new() { "Command" };
}
