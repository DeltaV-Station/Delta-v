using Content.Shared._DV.Medical.CrewMonitoring; // DeltaV
using Content.Shared.Medical.CrewMonitoring;
using Robust.Client.UserInterface;

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

        EntityUid? gridUid = null;
        var stationName = string.Empty;

        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var xform))
        {
            gridUid = xform.GridUid;
            // Begin DeltaV Additions - Find a station's grid instead of the current one for evil monitors
            if (EntMan.HasComponent<LongRangeCrewMonitorComponent>(Owner))
            {
                gridUid = EntMan.System<LongRangeCrewMonitorSystem>().FindStationGridInMap(xform.MapID);
                gridUid ??= xform.GridUid; // fall back to whatever grid this is on if it failed
            }
            // End DeltaV Additions

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
