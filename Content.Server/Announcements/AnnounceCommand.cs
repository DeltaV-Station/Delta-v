using System.Linq; // Impstation Random Announcer System
using Content.Server.Administration;
using Content.Server._EE.Announcements.Systems; // Impstation Random Announcer System
using Content.Shared.Administration;
using Content.Shared._EE.Announcements.Prototypes; // Impstation Random Announcer System
using Robust.Shared.Console;
using Robust.Shared.Player; // Impstation Random Announcer System
using Robust.Shared.Prototypes; // Impstation Random Announcer System

namespace Content.Server.Announcements;

[AdminCommand(AdminFlags.Moderator)]
public sealed class AnnounceCommand : LocalizedEntityCommands
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IResourceManager _res = default!;

    public override string Command => "announce";
    public override string Description => Loc.GetString("cmd-announce-desc");
    public override string Help => Loc.GetString("cmd-announce-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {

        public string Command => "announce";
        public string Description => "Send an in-game announcement.";
        public string Help => $"{Command} <sender> <message> <sound> <announcer>"; // Impstation Random Announcer System: Adds announcer
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var announcer = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<AnnouncerSystem>(); // Start Impstation Random Announcer System
            var proto = IoCManager.Resolve<IPrototypeManager>();

            switch (args.Length)
            {
                case 0:
                    shell.WriteError("Not enough arguments! Need at least 1.");
                    return;
                case 1:
                    announcer.SendAnnouncement(announcer.GetAnnouncementId("CommandReport"), Filter.Broadcast(),
                        args[0], "Central Command", Color.Gold);
                    break;
                case 2:
                    announcer.SendAnnouncement(announcer.GetAnnouncementId("CommandReport"), Filter.Broadcast(),
                        args[1], args[0], Color.Gold);
                    break;
                case 3:
                    announcer.SendAnnouncement(announcer.GetAnnouncementId(args[2]), Filter.Broadcast(), args[1],
                        args[0], Color.Gold);
                    break;
                case 4:
                    if (!proto.TryIndex(args[3], out AnnouncerPrototype? prototype))
                    {
                        shell.WriteError($"No announcer prototype with ID {args[3]} found!");
                        return;
                    }
                    announcer.SendAnnouncement(args[2], Filter.Broadcast(), args[1], args[0], Color.Gold, null,
                        prototype);
                    break;
            }

            shell.WriteLine("Sent!");
        }
        
        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            switch (args.Length)
            {
                case 3:
                {
                    var list = new List<string>();

                    foreach (var prototype in IoCManager.Resolve<IPrototypeManager>()
                                .EnumeratePrototypes<AnnouncerPrototype>()
                                .SelectMany<AnnouncerPrototype, string>(p => p.Announcements.Select(a => a.ID)))
                    {
                        if (!list.Contains(prototype))
                            list.Add(prototype);
                    }

                    return CompletionResult.FromHintOptions(list, Loc.GetString("admin-announce-hint-sound"));
                }
                case 4:
                {
                    var list = new List<string>();

                    foreach (var prototype in IoCManager.Resolve<IPrototypeManager>()
                        .EnumeratePrototypes<AnnouncerPrototype>())
                    {
                        if (!list.Contains(prototype.ID))
                            list.Add(prototype.ID);
                    }

                    return CompletionResult.FromHintOptions(list, Loc.GetString("admin-announce-hint-voice"));
                }
                default:
                    return CompletionResult.Empty; // End Impstation Random Announcer System
            }
        }

        // Optional sound argument
        if (args.Length >= 4)
            sound = new SoundPathSpecifier(args[3]);

        _chat.DispatchGlobalAnnouncement(message, sender, true, sound, color);
        shell.WriteLine(Loc.GetString("shell-command-success"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint(Loc.GetString("cmd-announce-arg-message")),
            2 => CompletionResult.FromHint(Loc.GetString("cmd-announce-arg-sender")),
            3 => CompletionResult.FromHint(Loc.GetString("cmd-announce-arg-color")),
            4 => CompletionResult.FromHintOptions(
                CompletionHelper.AudioFilePath(args[3], _proto, _res),
                Loc.GetString("cmd-announce-arg-sound")
            ),
            _ => CompletionResult.Empty
        };
    }
}
