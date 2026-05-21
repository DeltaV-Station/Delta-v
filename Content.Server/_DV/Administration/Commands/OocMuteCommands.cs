using Content.Server._DV.Administration.Components;
using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class MuteOocCommand : LocalizedCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IChatManager _chat = default!;

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

        (string, object)[] locArgs =
            [("player", session.Name), ("chat", "OOC"), ("admin", shell.Player?.Name ?? "Unknown")];

        _chat.SendAdminAlert(
            playerData.OocMuted
                ? Loc.GetString("ooc-mute-cmds-admin-notif-muted", locArgs)
                : Loc.GetString("ooc-mute-cmds-admin-notif-unmuted", locArgs));

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

[AdminCommand(AdminFlags.Ban)]
public sealed class MuteDeadchatCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    public override string Command => "mute_deadchat";

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
        mute.MuteDeadchat = !mute.MuteDeadchat;

        (string, object)[] locArgs =
            [("player", session.Name), ("chat", "Deadchat"), ("admin", shell.Player?.Name ?? "Unknown")];

        _chat.SendAdminAlert(
            mute.MuteDeadchat
                ? Loc.GetString("ooc-mute-cmds-admin-notif-muted", locArgs)
                : Loc.GetString("ooc-mute-cmds-admin-notif-unmuted", locArgs));

        _chat.DispatchServerMessage(session,
            mute.MuteDeadchat
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
