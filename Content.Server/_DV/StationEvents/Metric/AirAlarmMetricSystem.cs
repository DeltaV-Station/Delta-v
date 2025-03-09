using System.Linq;
using Content.Server._Goobstation.StationEvents.Metric.Components;
using Content.Server._Goobstation.StationEvents.Metric;
using Content.Server.Atmos.Monitor.Components;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared._Goobstation.StationEvents.Metric;
using Content.Shared.Atmos.Monitor;
using Content.Shared.FixedPoint;

namespace Content.Server._DV.StationEvents.Metric;

/// <summary>
///   Uses air alarms to sample station chaos across the station
/// </summary>
public sealed class AirAlarmMetricSystem : ChaosMetricSystem<AirAlarmMetricComponent>
{
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override ChaosMetrics CalculateChaos(EntityUid metric_uid, AirAlarmMetricComponent component,
        CalculateChaosEvent args)
    {
        var airAlarmCounter = FixedPoint2.Zero;
        var airAlarmBadCounter = FixedPoint2.Zero;

        var stationGrids = _stationSystem.GetAllStationGrids();

        var queryAirAlarm = EntityQueryEnumerator<AirAlarmComponent, TransformComponent>();
        while (queryAirAlarm.MoveNext(out var uid, out var alarm, out var transform))
        {
            if (transform.GridUid == null || !stationGrids.Contains(transform.GridUid.Value))
                continue;

            airAlarmBadCounter += component.Weights.GetValueOrDefault(alarm.State, FixedPoint2.Zero);
            airAlarmCounter += 1;
        }

        var atmosChaos = FixedPoint2.Zero;

        if (airAlarmCounter > FixedPoint2.Zero)
            atmosChaos = (airAlarmBadCounter / airAlarmCounter) * component.AirAlarmCost;

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, FixedPoint2>()
        {
            {ChaosMetric.Atmos, atmosChaos},
        });
        return chaos;
    }
}
