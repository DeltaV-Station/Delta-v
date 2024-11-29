using Content.Server.DeltaV.Station.Systems;

namespace Content.Server.DeltaV.Station.Components;

/// <summary>
/// Denotes a station has no captain and holds data for automatic ACO systems
/// </summary>
[RegisterComponent, Access(typeof(NoCaptainSystem))]
public sealed partial class NoCaptainComponent : Component
{
    /// <summary>
    /// How long with no captain before an ACO vote is requested
    /// </summary>
    [DataField]
    public TimeSpan RequestACOVoteDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How long after ACO vote is called the spare id cabinet will be unlocked if applicable
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
}
