using Content.Server.DeltaV.Station.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access;
using Robust.Shared.Prototypes;

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
    public bool HasCaptain;

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
    /// Used to denote that AA has been brought into the round either from captain or safe.
    /// </summary>
    [DataField]
    public bool IsAAInPlay;

    /// <summary>
    /// The localization ID for announcing that AA has been unlocked for ACO
    /// </summary>
    [DataField]
    public LocId AAUnlockedMessage = "no-captain-aa-unlocked-announcement";

    /// <summary>
    /// The access level to grant to spare ID cabinets
    /// </summary>
    [DataField]
    public ProtoId<AccessLevelPrototype> ACOAccess = "Command";
}
