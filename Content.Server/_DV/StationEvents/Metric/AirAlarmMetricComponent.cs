using Content.Shared.Atmos.Monitor;
using Content.Shared.FixedPoint;

namespace Content.Server._DV.StationEvents.Metric;

[RegisterComponent, Access(typeof(AirAlarmMetricSystem))]
public sealed partial class AirAlarmMetricComponent : Component
{
    /// <summary>
    ///   Cost of all air alarms going off
    /// </summary>
    [DataField]
    public float AirAlarmCost = 300.0f;

    /// <summary>
    ///   How much each air alarm state adds to the average
    /// </summary>
    [DataField]
    public Dictionary<AtmosAlarmType, FixedPoint2> Weights = new() {
        [AtmosAlarmType.Invalid] = 0f,
        [AtmosAlarmType.Normal] = 0f,
        [AtmosAlarmType.Warning] = 0.5f,
        [AtmosAlarmType.Danger] = 1f,
        [AtmosAlarmType.Emagged] = 1.5f,
    };
}
