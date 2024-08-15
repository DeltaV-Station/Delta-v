namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(RampingStationEventSchedulerSystem))]
public sealed partial class RampingStationEventSchedulerComponent : Component
{
    /// <summary>
    ///     Average ending chaos modifier for the ramping event scheduler. Higher means faster.
    ///     Max chaos chosen for a round will deviate from this
    /// </summary>
    [DataField]
    public float AverageChaos = 12f;

    /// <summary>
    ///     Average time (in minutes) for when the ramping event scheduler should stop increasing the chaos modifier.
    ///     Close to how long you expect a round to last, so you'll probably have to tweak this on downstreams.
    /// </summary>
    [DataField]
    public float AverageEndTime = 90f;

    [DataField]
    public float EndTime;

    [DataField("maxChaos"), ViewVariables(VVAccess.ReadWrite)]
    public float MaxChaos;

    [DataField("startingChaos"), ViewVariables(VVAccess.ReadWrite)]
    public float StartingChaos;

    [DataField("timeUntilNextEvent"), ViewVariables(VVAccess.ReadWrite)]
    public float TimeUntilNextEvent;
}
