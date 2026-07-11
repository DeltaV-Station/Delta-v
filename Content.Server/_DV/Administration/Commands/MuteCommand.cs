using Content.Server._DV.Administration.Components;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

/// <summary>
/// Command for muting/unmuting specific players in specific OOC channels.
/// Mutes are not persisted to storage.
/// </summary>
///
/// After adding/removing a channel in <see cref="Execute"/>, don't forget to:
/// - Update <see cref="CHANNEL_COMPLETION_OPTIONS"/>
/// - Document behavior in "cmd-mute-help" in Resources/Locale/en-US/_DV/administration/commands/mute.ftl

[AdminCommand(AdminFlags.Ban)]
public sealed class MuteCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IAdminLogManager _adminLogs = default!;

    private static readonly string[] ChannelCompletionOptions = ["OOC", "LOOC", "DEADCHAT"];
    private static readonly string ChannelListText = string.Join(", ", ChannelCompletionOptions);

    public override string Command => "mute";
    public override string Help => Loc.GetString("cmd-mute-help", ("channels", ChannelListText), ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!_player.TryGetSessionByUsername(args[1], out var session))
        {
            shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        bool isMuted;
        switch (args[0].ToLower())
        {
            case "deadchat":
            {
                if (session.GetMind() is not { } mind)
                {
                    shell.WriteLine(Loc.GetString("cmd-mute-err-missing-mind"));
                    return;
                }

                var mute = EntityManager.EnsureComponent<InGameOocMutedComponent>(mind);
                mute.MuteDeadchat = !mute.MuteDeadchat;
                isMuted = mute.MuteDeadchat;
            }
                break;

            case "looc":
            {
                if (session.GetMind() is not { } mind)
                {
                    shell.WriteLine(Loc.GetString("cmd-mute-err-missing-mind"));
                    return;
                }

                var mute = EntityManager.EnsureComponent<InGameOocMutedComponent>(mind);
                mute.MuteLooc = !mute.MuteLooc;
                isMuted = mute.MuteLooc;
            }
                break;

            case "ooc":
                if (session.ContentData() is not { } playerData)
                {
                    shell.WriteLine(Loc.GetString("cmd-mute-err-no-data"));
                    return;
                }

                playerData.OocMuted = !playerData.OocMuted;
                isMuted = playerData.OocMuted;
                break;

            default:
                shell.WriteLine(Loc.GetString("cmd-mute-err-unknown-channel", ("channels", ChannelListText)));
                return;
        }

        var channelName = args[0].ToUpper();
        _adminLogs.Add(LogType.AdminCommands,
            LogImpact.Extreme,
            $"{session:player} has been {(isMuted ? "muted" : "unmuted")} in {channelName} by {shell.Player?.Name ?? "<UNKNOWN>"}");

        var locArgs = ("chat", channelName);
        _chat.DispatchServerMessage(session,
            isMuted
                ? Loc.GetString("cmd-mute-player-notif-muted", locArgs)
                : Loc.GetString("cmd-mute-player-notif-unmuted", locArgs),
            true);

        shell.WriteLine(Loc.GetString("shell-command-success"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                return CompletionResult.FromHintOptions(ChannelCompletionOptions, "channel");
            case 2:
                return CompletionResult.FromOptions(CompletionHelper.SessionNames());
            default:
                return CompletionResult.Empty;
        }
    }
}
