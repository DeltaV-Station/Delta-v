using Content.Server._DV.Station.Systems;
using Content.Shared.Access;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Station.Components;

public enum AutomaticSpareIdState
{
    RoundStart,
    Alerted,
    AwaitingUnlock,
    Unlocked,
    CaptainPresent,
    WarOps
}

[RegisterComponent, Access(typeof(AutomaticSpareIdSystem)), AutoGenerateComponentPause]
public sealed partial class AutomaticSpareIdComponent : Component
{
    /// <summary>
    /// The current state of the automatic spare ID system
    /// </summary>
    [DataField]
    public AutomaticSpareIdState State = AutomaticSpareIdState.RoundStart;

    /// <summary>
    /// Timeout before an action is taken if the state doesn't change
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? Timeout;

    /// <summary>
    /// The job considered as Captain for the automatic spare ID system
    /// </summary>
    [DataField]
    public ProtoId<JobPrototype> CaptainJob = "Captain";

    /// <summary>
    /// The access that the spare ID safe will be extended to have if it is automatically unlocked
    /// if there is no captain
    /// </summary>
    [DataField]
    public ProtoId<AccessLevelPrototype> GrantAccessToCommand = "Command";

    /// <summary>
    /// The access that the spare ID safe will be extended to have if it is automatically unlocked
    /// if there is a captain
    /// </summary>
    [DataField]
    public ProtoId<AccessLevelPrototype> GrantAccessToCaptain = "Captain";

    /// <summary>
    /// Message for when a Captain joins after the system has alerted about their absence
    /// </summary>
    [DataField]
    public LocId CaptainPresentAfterAlertsMessage = "captain-arrived-revoke-aco-announcement";

    /// <summary>
    /// Message for when the system alerts but isn't going to automatically unlock
    /// </summary>
    [DataField]
    public LocId AlertedMessage = "no-captain-request-aco-vote-announcement";

    /// <summary>
    /// Message for when the system alerts and will automatically unlock
    /// </summary>
    [DataField]
    public LocId AwaitingUnlockMessage = "no-captain-request-aco-vote-with-aa-announcement";

    /// <summary>
    /// Message for when the system alerts automatically unlock
    /// </summary>
    [DataField]
    public LocId UnlockedMessage = "no-captain-aa-unlocked-announcement";

    /// <summary>
    /// The amount of time in which that the spare ID will unlock after nuclear operatives declare war.
    /// </summary>
    public TimeSpan WarOpsUnlockDelay = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Message that will be displayed to the station when there is no captain and war ops is declared.
    /// </summary>
    public LocId WarOpsUnlockedMessageACO = "spare-id-warops-no-captain";

    /// <summary>
    /// Message that will be displayed to the station when there is a captain and war ops is declared.
    /// </summary>
    public LocId WarOpsUnlockedMessageCaptain = "spare-id-warops-captain";
}
