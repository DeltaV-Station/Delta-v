using Content.Server._DV.Station.Components;
using Content.Server._DV.Station.Systems;
using Content.Server.AlertLevel;
using Content.Server.Communications;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Shared._DV.Communications;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Station;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Communications;

public sealed class DVCommunicationsConsoleSystem : SharedDVCommunicationsConsoleSystem
{
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly StationExfiltrationSystem _stationExfiltration = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndChanged);
        SubscribeLocalEvent<StationExfiltrationChangedEvent>(OnExfiltrationChanged);
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);
    }

    private void OnAlertLevelChanged(AlertLevelChangedEvent ev)
    {
        var query = EntityQueryEnumerator<DVCommunicationsConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_station.GetOwningStation(uid) != ev.Station)
                continue;

            var alertLevel = Comp<AlertLevelComponent>(ev.Station);
            comp.CanSetAlertAt = Timing.CurTime + TimeSpan.FromSeconds(alertLevel.CurrentDelay);
            comp.CurrentAlertLevel = ev.AlertLevel;
            if (alertLevel.IsLevelLocked)
                comp.CanSetAlertAt = null;
            Dirty(uid, comp);
        }
    }

    private void OnExfiltrationChanged(ref StationExfiltrationChangedEvent ev)
    {
        var query = EntityQueryEnumerator<DVCommunicationsConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_station.GetOwningStation(uid) != ev.Station)
                continue;

            comp.ExpectedExfiltrationArrival = ev.Station.Comp.ArrivalTime;
            Dirty(uid, comp);
        }
    }

    private void OnRoundEndChanged(RoundEndSystemChangedEvent ev)
    {
        var query = EntityQueryEnumerator<DVCommunicationsConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.ExpectedEvacuationArrival = _roundEnd.ExpectedCountdownEnd;
            comp.ExpectedEvacuationDuration = _roundEnd.ExpectedShuttleLength;
            comp.ShuttlesCallable = ShuttlesCallable();
            Dirty(uid, comp);
        }
    }

    protected override void OnMapInit(Entity<DVCommunicationsConsoleComponent> ent, ref MapInitEvent args)
    {
        base.OnMapInit(ent, ref args);

        if (_station.GetOwningStation(ent) is not { } station)
            return;

        if (!TryComp<AlertLevelComponent>(station, out var alertLevel))
            return;

        ent.Comp.CurrentAlertLevel = alertLevel.CurrentLevel;
        var proto = _prototype.Index<AlertLevelPrototype>(alertLevel.AlertLevelPrototype);
        foreach (var (name, detail) in proto.Levels)
        {
            ent.Comp.AlertLevels.Add(new($"alert-level-{name}", $"alert-level-{name}-announcement", name, !detail.DisableSelection, detail.Color));
        }
        ent.Comp.CanSetAlertAt = Timing.CurTime + TimeSpan.FromSeconds(alertLevel.CurrentDelay);
        if (alertLevel.IsLevelLocked)
            ent.Comp.CanSetAlertAt = null;
        ent.Comp.ShuttlesCallable = ShuttlesCallable();

        ent.Comp.ExpectedEvacuationArrival = _roundEnd.ExpectedCountdownEnd;
        ent.Comp.ExpectedEvacuationDuration = _roundEnd.ExpectedShuttleLength;
        if (TryComp<StationExfiltrationComponent>(station, out var exfiltration))
            ent.Comp.ExpectedExfiltrationArrival = exfiltration.ArrivalTime;

        Dirty(ent);
    }

    private bool ShuttlesCallable()
    {
        // Defer to what the round end system thinks we should be able to do.
        if (_emergencyShuttle.EmergencyShuttleArrived || !_roundEnd.CanCallOrRecall())
            return false;

        // Calling shuttle checks
        if (_roundEnd.ExpectedCountdownEnd is null)
            return true;

        // Recalling shuttle checks
        var recallThreshold = _configuration.GetCVar(CCVars.EmergencyRecallTurningPoint);

        // shouldn't really be happening if we got here
        if (_roundEnd.ShuttleTimeLeft is not { } left
            || _roundEnd.ExpectedShuttleLength is not { } expected)
            return false;

        return !(left.TotalSeconds / expected.TotalSeconds < recallThreshold);
    }

    protected override void OnAlertLevel(Entity<DVCommunicationsConsoleComponent> ent, ref DVCommunicationsConsoleAlertLevelMessage args)
    {
        base.OnAlertLevel(ent, ref args);

        if (!AccessReader.IsAllowed(ent, args.Actor))
            return;

        if (_station.GetOwningStation(ent) is not { } station)
            return;

        _alertLevel.SetLevel(station, args.AlertLevel, true, true);
        AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):player} has set the alert level to {args.AlertLevel:level} on {ToPrettyString(station):station} using {ToPrettyString(ent):console}");
    }

    protected override void OnEvacuationShuttle(Entity<DVCommunicationsConsoleComponent> ent, ref DVCommunicationsConsoleEvacuationShuttleMessage args)
    {
        base.OnEvacuationShuttle(ent, ref args);

        if (!AccessReader.IsAllowed(ent, args.Actor))
            return;

        if (args.Call)
        {
            var ev = new CommunicationConsoleCallShuttleAttemptEvent(ent, default!, args.Actor);
            RaiseLocalEvent(ref ev);

            _roundEnd.RequestRoundEnd(args.Actor);
            AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):player} has called the evacuation shuttle using {ToPrettyString(ent):console}");
        }
        else
        {
            _roundEnd.CancelRoundEndCountdown(args.Actor);
            AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):player} has recalled the evacuation shuttle using {ToPrettyString(ent):console}");
        }
    }

    protected override void OnExfiltrationShuttle(Entity<DVCommunicationsConsoleComponent> ent, ref DVCommunicationsConsoleExfiltrationShuttleMessage args)
    {
        base.OnExfiltrationShuttle(ent, ref args);

        if (!AccessReader.IsAllowed(ent, args.Actor))
            return;

        if (_station.GetOwningStation(ent) is not { } station)
            return;

        if (args.Call)
        {
            _stationExfiltration.Call(station);
            AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):player} has called the exfiltration shuttle using {ToPrettyString(ent):console}");
        }
        else
        {
            _stationExfiltration.Recall(station);
            AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):player} has recalled the exfiltration shuttle using {ToPrettyString(ent):console}");
        }
    }
}
