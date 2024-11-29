using Content.Server.DeltaV.Station.Systems;
using Content.Server.Station.Systems;
using System.ComponentModel.DataAnnotations;

namespace Content.Server.DeltaV.Station.Components;

/// <summary>
/// Denotes a station has no captain and holds data for automatic ACO systems
/// </summary>
[RegisterComponent, Access(typeof(CaptainStateSystem), typeof(StationSystem))]
public sealed partial class CaptainStateComponent : Component
{
    /// <summary>
    /// Denotes wether the entity has a captain or not
    /// </summary>
    /// <remarks>
    /// Assume no captain unless specified
    /// </remarks>
    [DataField]
    public bool HasCaptain = false;

    /// <summary>
    /// How long with no captain before an ACO is requested
    /// </summary>
    [DataField]
    public TimeSpan ACORequestDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Set after ACO has been requested to avoid duplicate calls
    /// </summary>
    [DataField]
    public bool ACORequestHandled = false;

    /// <summary>
    /// How long after ACO is requested the spare id cabinet will be unlocked if applicable
    /// </summary>
    [DataField]
    public TimeSpan UnlockAADelay = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum player count that AA should be unlocked for ACO
    /// </summary>
    /// <remarks>
    /// If there are too many players it's likely that there is a sufficient command staffing to render automatic aa approval redundant.
    /// </remarks>
    [DataField]
    public int UnlockAAPlayerThreshold = 40;

    /// <summary>
    /// Used to override other conditions and AA will be unlocked after the delay
    /// </summary>
    [DataField]
    public bool? UnlockAAOverride = null;

    /// <summary>
    /// Used to denote that a captain has left and lost job (i.e. cryo) as apposed to no captain since round start
    /// </summary>
    [DataField]
    public bool CaptainDeparted = false;
}
