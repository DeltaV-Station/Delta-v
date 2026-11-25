using Content.Server.AlertLevel;
using Content.Server.Screens.Components;
using Content.Shared._DV.Communications;
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
        if (args.Data.TryGetValue(DVScreenPackets.Text, out (string, string)? text))
            OnTextPacket(ent, text.Value, ref args);
        if (args.Data.TryGetValue(DVScreenPackets.ShowBorders, out bool? showBorders))
            OnBordersPacket(ent, showBorders.Value, ref args);
        if (args.Data.TryGetValue(DVScreenPackets.Content, out DVScreenContent? content))
            OnContentPacket(ent, content.Value, ref args);
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

    private void OnTextPacket(Entity<DVScreenComponent> ent, (string, string) text, ref DeviceNetworkPacketEvent args)
    {
        ent.Comp.Line1 = text.Item1;
        ent.Comp.Line2 = text.Item2;

        Dirty(ent);
        UpdateVisuals(ent);
    }

    private void OnBordersPacket(Entity<DVScreenComponent> ent, bool showBorders, ref DeviceNetworkPacketEvent args)
    {
        ent.Comp.ShowAlertBorder = showBorders;

        Dirty(ent);
        UpdateVisuals(ent);
    }

    private void OnContentPacket(Entity<DVScreenComponent> ent, DVScreenContent content, ref DeviceNetworkPacketEvent args)
    {
        ent.Comp.Content = content;

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
