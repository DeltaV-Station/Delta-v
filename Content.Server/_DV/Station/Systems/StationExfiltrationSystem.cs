using System.Numerics;
using Content.Server._DV.Station.Components;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Pinpointer;
using Content.Server.Screens.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork;
using Content.Shared.Localizations;
using Content.Shared.Shuttles.Components;
using Content.Shared.Tiles;
using Robust.Shared.Audio;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._DV.Station.Systems;

public sealed class StationExfiltrationSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly DockingSystem _docking = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly CommunicationsConsoleSystem _communicationsConsole = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationExfiltrationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.ArrivalTime && comp.SpawnedShuttle is not null)
            {
                DockShuttle((uid, comp));
            }
            if (_timing.CurTime >= comp.ImpendingDepartureAnnouncementTime)
            {
                _chat.DispatchGlobalAnnouncement(
                    Loc.GetString(comp.AboutToLeaveAnnouncement, ("time", comp.DepartureWarningTime.TotalSeconds), ("station", Name(uid))),
                    sender: Loc.GetString(comp.Sender),
                    colorOverride: Color.Red);
                comp.ImpendingDepartureAnnouncementTime = null;
            }
            if (_timing.CurTime >= comp.DepartureTime)
            {
                _chat.DispatchGlobalAnnouncement(
                    Loc.GetString(comp.LeftAnnouncement, ("station", Name(uid))),
                    sender: Loc.GetString(comp.Sender),
                    colorOverride: Color.Gold);
                comp.DepartureTime = null;
                QueueDel(comp.SpawnedShuttle);
                comp.SpawnedShuttle = null;
            }
        }
    }

    private bool PrepareShuttle(Entity<StationExfiltrationComponent> ent)
    {
        if (ent.Comp.SpawnedShuttle is not null)
            return true;

        if (!TryComp<StationCentcommComponent>(ent, out var centcomm) || !TryComp<MapComponent>(centcomm.MapEntity, out var centcommMap))
            return false;

        if (!_loader.TryLoadGrid(centcommMap.MapId,
            ent.Comp.ShuttlePath,
            out var shuttle,
            offset: new Vector2(-1000f, 0f)))
        {
            Log.Error($"Unable to spawn exfiltration shuttle {ent.Comp.ShuttlePath} for {ToPrettyString(ent)}");
            return false;
        }

        ent.Comp.SpawnedShuttle = shuttle.Value;
        EnsureComp<ProtectedGridComponent>(shuttle.Value);
        EnsureComp<PreventPilotComponent>(shuttle.Value);

        return true;
    }

    private void DockShuttle(Entity<StationExfiltrationComponent> ent)
    {
        if (ent.Comp.SpawnedShuttle is not { } shuttle)
            return;

        if (!TryComp<ShuttleComponent>(shuttle, out var shuttleComp))
            return;

        if (!TryComp<StationDataComponent>(ent, out var stationData))
            return;

        if (_station.GetLargestGrid(stationData) is not { } grid)
            return;

        if (!TryComp<StationCentcommComponent>(ent, out var centcomm))
            return;

        ent.Comp.ArrivalTime = null;

        var ok = _shuttle.TryFTLDock(shuttle, shuttleComp, grid, out var config, ent.Comp.DockTo);

        var angle = _docking.GetAngle(shuttle, Transform(shuttle), grid, Transform(grid));

        var direction = ContentLocalizationManager.FormatDirection(angle.GetDir());
        var location = FormattedMessage.RemoveMarkupPermissive(
            _navMap.GetNearestBeaconString((shuttle, Transform(shuttle))));

        var locKey = ok ? ent.Comp.DockedAtStationAnnouncement : ent.Comp.DockedNearbyStationAnnouncement;

        _chat.DispatchStationAnnouncement(
            ent,
            Loc.GetString(
                locKey,
                ("time", $"{ent.Comp.LeaveTime.TotalSeconds}"),
                ("direction", direction),
                ("location", location),
                ("station", Name(ent))),
            sender: Loc.GetString(ent.Comp.Sender),
            colorOverride: Color.Gold);

        if (TryComp<DeviceNetworkComponent>(shuttle, out var netComp))
        {
            var payload = new NetworkPayload
            {
                [ShuttleTimerMasks.ShuttleMap] = shuttle,
                [ShuttleTimerMasks.SourceMap] = Transform(grid).MapUid,
                [ShuttleTimerMasks.DestMap] = centcomm.MapEntity,
                [ShuttleTimerMasks.ShuttleTime] = ent.Comp.LeaveTime,
                [ShuttleTimerMasks.SourceTime] = ent.Comp.LeaveTime,
                [ShuttleTimerMasks.DestTime] = ent.Comp.LeaveTime,
                [ShuttleTimerMasks.Docked] = true,
            };
            _deviceNetwork.QueuePacket(shuttle, null, payload, netComp.TransmitFrequency);
        }

        ent.Comp.ImpendingDepartureAnnouncementTime = _timing.CurTime + ent.Comp.LeaveTime - ent.Comp.DepartureWarningTime;
        ent.Comp.DepartureTime = _timing.CurTime + ent.Comp.LeaveTime;
    }

    public void Call(Entity<StationExfiltrationComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        if (!PrepareShuttle((ent.Owner, ent.Comp)))
        {
            _chat.DispatchStationAnnouncement(ent, Loc.GetString(ent.Comp.FailedAnnouncement, ("station", Name(ent))), sender: Loc.GetString(ent.Comp.Sender), colorOverride: Color.Gold);
        }
        else
        {
            ent.Comp.ArrivalTime = _timing.CurTime + ent.Comp.TravelTime;
            _chat.DispatchStationAnnouncement(ent, Loc.GetString(ent.Comp.CalledAnnouncement, ("time", ent.Comp.TravelTime.TotalSeconds), ("station", Name(ent))), sender: Loc.GetString(ent.Comp.Sender), colorOverride: Color.Gold);
        }

        _communicationsConsole.UpdateCommsConsoleInterface();
    }

    public void Recall(Entity<StationExfiltrationComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        ent.Comp.ArrivalTime = null;
        _chat.DispatchStationAnnouncement(ent, Loc.GetString(ent.Comp.RecalledAnnouncement, ("station", Name(ent))), sender: Loc.GetString(ent.Comp.Sender), colorOverride: Color.Gold);

        _communicationsConsole.UpdateCommsConsoleInterface();
    }
}
