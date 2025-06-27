using Content.Shared.Medical.CrewMonitoring;
using Robust.Client.UserInterface;
using Content.Client.Station;
using System.Diagnostics;
using System.Linq;
using Content.Shared.Station.Components;
using Robust.Shared.Log;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

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
        // Delta-v start of crew monitor display correction
        var tagcomp = EntMan.GetComponent<TagComponent>(Owner).Tags.ToArray();
        EntityUid mappedGrid = default!;
        if (Array.Exists(tagcomp, v => v.Id == "Syndicate")) // Crew monitor marked as belonging to LPO or syndicate agent
        {
            var station = EntMan.EntitySysManager.GetEntitySystem<StationSystem>().Stations;
            var station0 = station.FirstOrDefault();
            var station0_grid0 = station0.StationGrids[0];
            mappedGrid = EntMan.GetEntity(station0_grid0);
        }
        else // continue as usual...
        {
            mappedGrid = Owner;
        }
        // end delta-v
        if (EntMan.TryGetComponent<TransformComponent>(mappedGrid, out var xform))
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
