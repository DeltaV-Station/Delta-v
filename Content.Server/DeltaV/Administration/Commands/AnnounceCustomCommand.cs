using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Robust.Shared.Audio;
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class AnnounceCustomCommand : IConsoleCommand
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IResourceManager _res = default!;

    public string Command => "announcecustom";
    public string Description => Loc.GetString("cmd-announcecustom-desc");
    public string Help => Loc.GetString("cmd-announcecustom-help", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();

        switch (args.Length)
        {
            case 0:
                shell.WriteError(Loc.GetString("shell-need-minimum-one-argument"));
                return;
            case > 4:
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
        }

        var message = args[0];
        var sender = "Central Command";
        var color = Color.Gold;
        var sound = new SoundPathSpecifier("/Audio/Announcements/announce.ogg");

        // Optional sender argument
        if (args.Length >= 2)
            sender = args[1];

        // Optional color argument
        if (args.Length >= 3)
        {
            try
            {
                color = Color.FromHex(args[2]);
            }
            catch
            {
                shell.WriteError(Loc.GetString("shell-invalid-color-hex"));
                return;
            }
        }

        // Optional sound argument
        if (args.Length >= 4)
            sound = new SoundPathSpecifier(args[3]);

        chat.DispatchGlobalAnnouncement(message, sender, true, sound, color);
        shell.WriteLine(Loc.GetString("shell-command-success"));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint(Loc.GetString("cmd-announcecustom-arg-message")),
            2 => CompletionResult.FromHint(Loc.GetString("shell-argument-username-optional-hint")),
            3 => CompletionResult.FromHint(Loc.GetString("cmd-announcecustom-arg-color")),
            4 => CompletionResult.FromHintOptions(
                CompletionHelper.AudioFilePath(args[3], _protoManager, _res),
                Loc.GetString("cmd-announcecustom-arg-sound")
            ),
            _ => CompletionResult.Empty
        };
    }
}
