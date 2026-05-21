using Content.Server._DV.Administration.Components;
using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class MuteLooc : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    public override string Command => "mute_looc";

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


        if (session.GetMind() is not { } mind)
        {
            shell.WriteLine(Loc.GetString("ooc-mute-cmds-err-missing-mind"));
            return;
        }

        var mute = _entity.EnsureComponent<InGameOocMutedComponent>(mind);
        mute.MuteLooc = !mute.MuteLooc;

        (string, object)[] locArgs =
            [("player", session.Name), ("chat", "LOOC"), ("admin", shell.Player?.Name ?? "Unknown")];

        _chat.SendAdminAlert(
            mute.MuteLooc
                ? Loc.GetString("ooc-mute-cmds-admin-notif-muted", locArgs)
                : Loc.GetString("ooc-mute-cmds-admin-notif-unmuted", locArgs));

        _chat.DispatchServerMessage(session,
            mute.MuteLooc
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
