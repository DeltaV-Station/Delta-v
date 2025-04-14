using Content.Shared.EntityTable.EntitySelectors;
using System.Numerics; // DeltaV

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(RampingStationEventSchedulerSystem))]
public sealed partial class RampingStationEventSchedulerComponent : Component
{
    /* DeltaV
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

    [DataField]
    public float MaxChaos;

    [DataField]
    public float StartingChaos;
    */

    /// <summary>
    ///     DeltaV - Key points which determine during what period will events last on average.
    ///     X is duration of the setting in minutes. Y is time until next event in minutes.
    /// </summary>
    [DataField(required: true)]
    public List<Vector2> TimeKeyPoints = new()
    {
        new Vector2(0f, 1f)
    };

    /// <summary>
    ///     DeltaV - Maximum possible error when randomly offsetting time until next event.
    /// </summary>
    [DataField]
    public float TimeDeviation = 1;

    [DataField]
    public float TimeUntilNextEvent;

    /// <summary>
    /// The gamerules that the scheduler can choose from
    /// </summary>
    /// Reminder that though we could do all selection via the EntityTableSelector, we also need to consider various <see cref="StationEventComponent"/> restrictions.
    /// As such, we want to pass a list of acceptable game rules, which are then parsed for restrictions by the <see cref="EventManagerSystem"/>.
    [DataField(required: true)]
    public EntityTableSelector ScheduledGameRules = default!;
}
