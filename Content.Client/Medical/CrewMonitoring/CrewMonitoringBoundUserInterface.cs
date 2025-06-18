using Content.Shared.Medical.CrewMonitoring;
using Robust.Client.UserInterface;
using Content.Client.Station;
using System.Diagnostics;
using System.Linq;
using Content.Shared.Station.Components;
using Robust.Shared.Log;

namespace Content.Client.Medical.CrewMonitoring;

public sealed class CrewMonitoringBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private CrewMonitoringWindow? _menu;

    public CrewMonitoringBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        var debug = Logger.GetSawmill("crewmon-ui");
        EntityUid? gridUid = null;
        var stationName = string.Empty;
        // Delta-v start of crew monitor display correction
        var station = EntMan.EntitySysManager.GetEntitySystem<StationSystem>().Stations;
        var station0 = station.FirstOrDefault();
        
        debug.Debug($"Stations: {station.Count} Station 0 name: {station0.Name}");
        var station0_uid = EntMan.GetEntity(station0.Entity);
        debug.Debug($"Station 0 ENT-UID: {station0_uid}, station 0 netid: {station0.Entity.Id}");
        
        if (EntMan.TryGetComponent<TransformComponent>(station0_uid, out var xform))
        {
            gridUid = xform.GridUid;

            if (EntMan.TryGetComponent<MetaDataComponent>(gridUid, out var metaData))
            {
                stationName = metaData.EntityName;
            }
        }
        

        _menu = this.CreateWindow<CrewMonitoringWindow>();
        _menu.Set(stationName, gridUid);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CrewMonitoringState st:
                EntMan.TryGetComponent<TransformComponent>(Owner, out var xform);
                _menu?.ShowSensors(st.Sensors, Owner, xform?.Coordinates);
                break;
        }
    }
}
