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
    /// Holds the round time of the last time a captain was present if one is not present currently
    /// </summary>
    [DataField]
    public TimeSpan TimeSinceCaptain = TimeSpan.Zero;

    /// <summary>
    /// How long with no captain before an ACO is requested
    /// </summary>
    [DataField]
    public TimeSpan ACORequestDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Set after ACO has been requested to avoid duplicate calls
    /// </summary>
    [DataField]
    public bool IsACORequestActive = false;

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
    /// Used to denote that a captain has left and lost job (i.e. cryo) as apposed to no captain since round start
    /// </summary>
    [DataField]
    public bool CaptainDeparted = false;
}
