using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class GetPingCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public override string Command => "getping";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !_player.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteError(Loc.GetString("cmd-getping-err"));
            return;
        }

        shell.WriteLine($"{session.Name}'s ping: {session.Ping}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "username")
            : CompletionResult.Empty;
    }
}
