using Content.Server.AlertLevel;
using Content.Server.Screens.Components;
using Content.Shared._DV.Screens;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Station;
using Robust.Shared.Timing;

namespace Content.Server._DV.Screens;

public sealed class DVScreenSystem : DVSharedScreenSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVScreenComponent, DeviceNetworkPacketEvent>(OnPacket);
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);
    }

    private void OnPacket(Entity<DVScreenComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (args.Data.TryGetValue(ShuttleTimerMasks.ShuttleMap, out _))
            OnShuttlePacket(ent, ref args);
        else if (args.Data.TryGetValue(ScreenMasks.Text, out string? text))
            OnTextPacket(ent, text, ref args);
    }

    private void OnShuttlePacket(Entity<DVScreenComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        var xform = Transform(ent);

        args.Data.TryGetValue(ShuttleTimerMasks.ShuttleMap, out EntityUid? shuttleMap);
        args.Data.TryGetValue(ShuttleTimerMasks.SourceMap, out EntityUid? source);
        args.Data.TryGetValue(ShuttleTimerMasks.DestMap, out EntityUid? dest);
        args.Data.TryGetValue(ShuttleTimerMasks.Docked, out bool docked);
        var screenIsAtDestination = docked;
        string key;

        switch (xform.MapUid)
        {
            // sometimes the timer transforms on FTL shuttles have a hyperspace mapuid, so matching by grid works as a fallback.
            case var local when local == shuttleMap || xform.GridUid == shuttleMap:
                key = ShuttleTimerMasks.ShuttleTime;
                break;
            case var origin when origin == source:
                key = ShuttleTimerMasks.SourceTime;
                break;
            case var remote when remote == dest:
                key = ShuttleTimerMasks.DestTime;
                screenIsAtDestination = false;
                break;
            default:
                return;
        }

        if (!args.Data.TryGetValue(key, out TimeSpan duration))
            return;

        ent.Comp.ScreenIsAtDestination = screenIsAtDestination;
        ent.Comp.TargetTime = _timing.CurTime + duration;
        Dirty(ent);
        UpdateVisuals(ent);
    }

    private void OnTextPacket(Entity<DVScreenComponent> ent, string text, ref DeviceNetworkPacketEvent args)
    {
        var lines = text.Split('\n');
        if (lines.Length >= 2)
        {
            ent.Comp.Line1 = lines[0];
            ent.Comp.Line2 = lines[1];
        }
        else
        {
            ent.Comp.Line1 = text;
            ent.Comp.Line2 = string.Empty;
        }
        Dirty(ent);
        UpdateVisuals(ent);
    }

    private void OnAlertLevelChanged(AlertLevelChangedEvent ev)
    {
        var query = EntityQueryEnumerator<DVScreenComponent>();
        while (query.MoveNext(out var uid, out var screen))
        {
            if (_station.GetOwningStation(uid) != ev.Station)
                continue;

            screen.AlertLevel = ev.AlertLevel;
            Dirty(uid, screen);
            UpdateVisuals((uid, screen));
        }
    }
}
