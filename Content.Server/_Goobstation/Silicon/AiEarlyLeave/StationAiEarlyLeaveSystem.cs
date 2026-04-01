using Content.Shared._Goobstation.Silicon.AiEarlyLeave;
using Content.Shared._Goobstation.Silicon.Components;
using Linguini.Bundle.Errors;

using Content.Server.Chat.Systems;
using Robust.Shared.Player;
using Content.Server.EUI;
using Robust.Shared.Network;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Silicons.StationAi;

namespace Content.Server._Goobstation.Silicon.AiEarlyLeave;

public sealed class StationAiEarlyLeaveSystem : SharedStationAiEarlyLeaveSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly StationJobsSystem _jobs = default!;
    [Dependency] private readonly StationSystem _station = default!;

    protected override void RequestEarlyLeave(Entity<StationAiCoreComponent> aiCore, EntityUid insertedAi)
    {
        if (!_player.TryGetSessionByEntity(insertedAi, out var aiSession))
            return;

        if (aiSession == null)
            return;

        _euiManager.OpenEui(new StationAiEarlyLeaveEui(aiCore, insertedAi, aiSession.UserId, this), aiSession);
    }

    public void EarlyLeave(Entity<StationAiCoreComponent> aiCore, EntityUid insertedAi, NetUserId userId)
    {
        var station = _station.GetOwningStation(insertedAi);

        // removes all of player's jobs on all stations
        foreach (var uniqueStation in _station.GetStationsSet())
        {
            if (!TryComp<StationJobsComponent>(uniqueStation, out var stationJobs))
                continue;

            if (!_jobs.TryGetPlayerJobs(uniqueStation, userId, out var jobs, stationJobs))
                continue;

            foreach (var job in jobs)
            {
                _jobs.TryAdjustJobSlot(uniqueStation, job, 1, clamp: true);
            }

            _jobs.TryRemovePlayerJobs(uniqueStation, userId, stationJobs);
        }

        if (station is not { })
            return;
        // Start of DeltaV Changes
        var message = Loc.GetString(
            "station-ai-earlyleave-announcement",
            ("character", Name(insertedAi)),
            ("entity", insertedAi)
        );

        _chat.DispatchStationAnnouncement(insertedAi, message, Loc.GetString("station-ai-earlyleave-announcement-sender"));
        // End of DeltaV Changes
        QueueDel(insertedAi);
    }
}
