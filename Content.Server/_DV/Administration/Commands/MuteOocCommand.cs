using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class MuteOocCommand : LocalizedCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IAdminLogManager _adminLogs = default!;

    public override string Command => "mute_ooc";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!_player.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        if (session.ContentData() is not { } playerData)
        {
            shell.WriteLine(Loc.GetString("cmd-mute_ooc-err-no-data"));
            return;
        }

        playerData.OocMuted = !playerData.OocMuted;

        _adminLogs.Add(LogType.AdminCommands,
            LogImpact.Extreme,
            $"{session.Name} has been {(playerData.OocMuted ? "muted" : "unmuted")} in OOC by {shell.Player?.Name ?? "Unknown"}");

        var locArgs = ("chat", "OOC");
        _chat.DispatchServerMessage(session,
            playerData.OocMuted
                ? Loc.GetString("ooc-mute-cmds-player-notif-muted", locArgs)
                : Loc.GetString("ooc-mute-cmds-player-notif-unmuted", locArgs),
            true);

        shell.WriteLine(Loc.GetString("shell-command-success"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromOptions(CompletionHelper.SessionNames())
            : CompletionResult.Empty;
    }
}
