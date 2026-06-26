using Content.Server.RoundEnd; // DeltaV
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(ZombieRuleSystem))]
public sealed partial class ZombieRuleComponent : Component
{
    /// <summary>
    /// When the round will next check for round end.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextRoundEndCheck;

    /// <summary>
    /// The amount of time between each check for the end of the round.
    /// </summary>
    [DataField]
    public TimeSpan EndCheckDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// After this amount of the crew become zombies, the shuttle will be automatically called.
    /// </summary>
    [DataField]
    public float ZombieShuttleCallPercentage = 0.7f;

    /// <summary>
    /// DeltaV - The behavior of the round if all zombies are defeated.
    /// </summary>
    public RoundEndBehavior ZombieRoundEndBehavior = RoundEndBehavior.ShuttleCall;

    /// <summary>
    /// DeltaV - The amount of time before the evac shuttle will arrive if the ZombieRoundEndBehavior is set to ShuttleCall.
    /// </summary>
    public TimeSpan EvacShuttleTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// DeltaV - The announcement sender when calling the shuttle.
    /// </summary>
    public string RoundEndTextSender = "comms-console-announcement-title-centcom";

    /// <summary>
    /// DeltaV - The announcement text when calling a shuttle.
    /// </summary>
    public string RoundEndTextShuttleCall = "zombie-no-more-threat-announcement-shuttle-call";

    /// <summary>
    /// DeltaV - The announcement text when a shuttle is already called.
    /// </summary>
    public string RoundEndTextAnnouncement = "zombie-no-more-threat-announcement";
}
