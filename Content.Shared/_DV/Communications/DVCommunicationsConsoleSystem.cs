using Content.Shared._DV.KeycardAuthenticationDevice;
using Content.Shared._DV.Screens;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Communications;

public abstract class SharedDVCommunicationsConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] protected readonly AccessReaderSystem AccessReader = default!;
    [Dependency] protected readonly ISharedAdminLogManager AdminLog = default!;
    [Dependency] private readonly SharedDVStationKeycardAuthenticationDeviceSystem _stationKeycardAuthenticationDevice = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVCommunicationsConsoleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DVCommunicationsConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceive);
        Subs.BuiEvents<DVCommunicationsConsoleComponent>(DVCommunicationsConsoleUi.Key,
            subs =>
            {
                subs.Event<DVCommunicationsConsoleEvacuationShuttleMessage>(OnEvacuationShuttle);
                subs.Event<DVCommunicationsConsoleExfiltrationShuttleMessage>(OnExfiltrationShuttle);
                subs.Event<DVCommunicationsConsoleScreenConfigurationMessage>(OnConfiguration);
                subs.Event<DVCommunicationsConsoleAnnouncementMessage>(OnAnnouncement);
                subs.Event<DVCommunicationsConsoleAlertLevelMessage>(OnAlertLevel);
                subs.Event<DVCommunicationsConsoleKeycardAuthenticationDeviceMessage>(OnKeycardAuthenticationDevice);
            });
    }

    private void OnPacketReceive(Entity<DVCommunicationsConsoleComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (args.Data.TryGetValue(DVScreenPackets.Text, out (string, string)? text))
        {
            ent.Comp.LastConfiguredLine1 = text.Value.Item1;
            ent.Comp.LastConfiguredLine2 = text.Value.Item2;
            Dirty(ent);
        }
        if (args.Data.TryGetValue(DVScreenPackets.ShowBorders, out bool? showBorders))
        {
            ent.Comp.LastConfiguredShowBorders = showBorders.Value;
            Dirty(ent);
        }
        if (args.Data.TryGetValue(DVScreenPackets.Content, out DVScreenContent? content))
        {
            ent.Comp.LastConfiguredContent = content.Value;
            Dirty(ent);
        }
    }

    protected virtual void OnMapInit(Entity<DVCommunicationsConsoleComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.CanAnnounceAt = Timing.CurTime + ent.Comp.InitialAnnouncementDelay;
    }

    private void OnAnnouncement(Entity<DVCommunicationsConsoleComponent> ent, ref DVCommunicationsConsoleAnnouncementMessage args)
    {
        if (!ent.Comp.CanAnnounce)
            return;

        if (Timing.CurTime <= ent.Comp.CanAnnounceAt)
            return;

        if (!AccessReader.IsAllowed(args.Actor, ent))
            return;

        var identity = new TryGetIdentityShortInfoEvent(ent, args.Actor);
        RaiseLocalEvent(identity);

        Loc.TryGetString(ent.Comp.AnnouncementTitle, out var title);
        title ??= ent.Comp.AnnouncementTitle;

        var msg = args.Announcement;
        msg += "\n" + Loc.GetString("comms-console-announcement-sent-by") + " " + identity.Title;

        if (ent.Comp.GlobalAnnouncements)
        {
            _chat.DispatchGlobalAnnouncement(msg, title, announcementSound: ent.Comp.AnnouncementSound, colorOverride: ent.Comp.AnnouncementColor);
            AdminLog.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(args.Actor):player} sent the following global announcement using {ToPrettyString(ent):console}: {msg:message}");
        }
        else
        {
            _chat.DispatchStationAnnouncement(ent, msg, title, announcementSound: ent.Comp.AnnouncementSound, colorOverride: ent.Comp.AnnouncementColor);
            AdminLog.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(args.Actor):player} sent the following station announcement using {ToPrettyString(ent):console}: {msg:message}");
        }

        ent.Comp.CanAnnounceAt = Timing.CurTime + ent.Comp.AnnouncementInterval;
        Dirty(ent);
    }

    private void OnConfiguration(Entity<DVCommunicationsConsoleComponent> ent, ref DVCommunicationsConsoleScreenConfigurationMessage args)
    {
        if (!ent.Comp.CanConfigureScreens)
            return;

        if (!AccessReader.IsAllowed(args.Actor, ent))
            return;

        ent.Comp.LastConfiguredLine1 = args.Line1;
        ent.Comp.LastConfiguredLine2 = args.Line2;
        ent.Comp.LastConfiguredShowBorders = args.ShowBorder;
        ent.Comp.LastConfiguredContent = args.Content;
        Dirty(ent);

        var payload = new NetworkPayload
        {
            [DVScreenPackets.Content] = args.Content,
            [DVScreenPackets.ShowBorders] = args.ShowBorder,
            [DVScreenPackets.Text] = (args.Line1, args.Line2),
        };

        AdminLog.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(args.Actor):player} configured the following text using {ToPrettyString(ent):console}: {args.Line1:line1} {args.Line2:line2}");
        _deviceNetwork.QueuePacket(ent, null, payload);
    }

    protected virtual void OnExfiltrationShuttle(Entity<DVCommunicationsConsoleComponent> ent,
        ref DVCommunicationsConsoleExfiltrationShuttleMessage args)
    {
    }

    protected virtual void OnEvacuationShuttle(Entity<DVCommunicationsConsoleComponent> ent,
        ref DVCommunicationsConsoleEvacuationShuttleMessage args)
    {
    }

    protected virtual void OnAlertLevel(Entity<DVCommunicationsConsoleComponent> ent,
        ref DVCommunicationsConsoleAlertLevelMessage args)
    {
    }

    private void OnKeycardAuthenticationDevice(Entity<DVCommunicationsConsoleComponent> ent,
        ref DVCommunicationsConsoleKeycardAuthenticationDeviceMessage args)
    {
        _stationKeycardAuthenticationDevice.TrySwipe(args.Actor, ent, args.Action);
    }
}
