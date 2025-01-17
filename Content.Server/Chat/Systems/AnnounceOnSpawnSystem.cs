using Content.Server.Chat;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Robust.Shared.Player; // Impstation Random Announcer System

namespace Content.Server.Chat.Systems;

public sealed class AnnounceOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Impstation Random Announcer System

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnnounceOnSpawnComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(EntityUid uid, AnnounceOnSpawnComponent comp, MapInitEvent args)
    {
        var sender = comp.Sender != null ? Loc.GetString(comp.Sender) : Loc.GetString("chat-manager-sender-announcement");
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId("SpawnAnnounceCaptain"), Filter.Broadcast(), // Impstation Random Announcer System: The announcer themselves
            comp.Message, sender, comp.Color);
    }
}
