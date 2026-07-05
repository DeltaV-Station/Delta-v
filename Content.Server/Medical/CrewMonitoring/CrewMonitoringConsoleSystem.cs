using System.Linq;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Power.EntitySystems; // DeltaV
using Content.Shared.PowerCell;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Medical.CrewMonitoring;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Pinpointer;
using Robust.Server.GameObjects;
using Robust.Shared.Audio; // DeltaV
using Robust.Shared.Audio.Systems; // DeltaV
using Robust.Shared.Timing; // DeltaV

namespace Content.Server.Medical.CrewMonitoring;

public sealed class CrewMonitoringConsoleSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!; // DeltaV
    [Dependency] private readonly IGameTiming _timing = default!; // DeltaV

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnRemove(EntityUid uid, CrewMonitoringConsoleComponent component, ComponentRemove args)
    {
        component.ConnectedSensors.Clear();
    }

    private void OnPacketReceived(EntityUid uid, CrewMonitoringConsoleComponent component, DeviceNetworkPacketEvent args)
    {
        var payload = args.Data;

        // Check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;

        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return;

        if (!payload.TryGetValue(SuitSensorConstants.NET_STATUS_COLLECTION, out Dictionary<string, SuitSensorStatus>? sensorStatus))
            return;

        component.ConnectedSensors = sensorStatus;
        UpdateUserInterface(uid, component);

        // DeltaV - start of alert system code
        if (!component.AlertsEnabled)
            return;

        // station power (for the machine version)
        if (!this.IsPowered(uid, EntityManager))
            return;

        // cell power (for the handheld)
        if (!_cell.HasActivatableCharge(uid))
            return;

        foreach (var (sensorId, status) in sensorStatus)
        {
            // DamagePercentage above 1f is considered critical. It is null when sensor vitals are off.
            var isCritical = status.DamagePercentage is >= 1f;

            // Skip crew members that we have already alerted about
            if (component.AlertedSensors.Contains(sensorId))
            {
                if (status.IsAlive && !isCritical)
                    component.AlertedSensors.Remove(sensorId);
                continue;
            }

            if (!status.IsAlive || isCritical)
            {
                if (_timing.CurTime >= component.NextAlert)
                {
                    var audioParams = AudioParams.Default.WithVolume(-2f).WithMaxDistance(4f);
                    _audio.PlayPvs(component.AlertSound, uid, audioParams);
                    component.NextAlert = _timing.CurTime + component.AlertCooldown;
                }

                // We are doing this outside the cooldown check to avoid "alert queues"
                // If two people die at the same time and remain dead for longer, we want to alert once for both people
                // instead of alerting once for the first one, waiting the cooldown, and then alerting again for the second one.
                component.AlertedSensors.Add(sensorId);
            }
        }
        // DeltaV - end of alert system code
    }

    private void OnUIOpened(EntityUid uid, CrewMonitoringConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (!_cell.TryUseActivatableCharge(uid))
            return;

        UpdateUserInterface(uid, component);
    }

    private void UpdateUserInterface(EntityUid uid, CrewMonitoringConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_uiSystem.IsUiOpen(uid, CrewMonitoringUIKey.Key))
            return;

        // The grid must have a NavMapComponent to visualize the map in the UI
        var xform = Transform(uid);

        if (xform.GridUid != null)
            EnsureComp<NavMapComponent>(xform.GridUid.Value);

        // Update all sensors info
        var allSensors = component.ConnectedSensors.Values.ToList();
        _uiSystem.SetUiState(uid, CrewMonitoringUIKey.Key, new CrewMonitoringState(allSensors));
    }
}
