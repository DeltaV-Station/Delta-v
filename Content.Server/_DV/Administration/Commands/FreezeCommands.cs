using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

// Keeping these as separate commands instead of toggling in one command so it's harder to unfreeze someone by mistake

[AdminCommand(AdminFlags.Admin)]
public sealed class FreezeCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly AdminFrozenSystem _frozen = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override string Command => "freeze";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        foreach (var username in args)
        {
            if (!_player.TryGetSessionByUsername(username, out var session))
            {
                shell.WriteError(Loc.GetString("freeze-cmds-err-not-found", ("username", username)));
                continue;
            }

            if (session.AttachedEntity is { } uid)
            {
                continue;
            }

            if (!_entity.HasComponent<AdminFrozenComponent>(uid))
            {
                _frozen.FreezeAndMute(uid);
                shell.WriteLine(Loc.GetString("cmd-freeze-success", ("username", username)));
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-freeze-err-already-frozen", ("username", username)));
            }
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "username");
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class UnfreezeCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override string Command => "unfreeze";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        foreach (var username in args)
        {
            if (!_player.TryGetSessionByUsername(username, out var session))
            {
                shell.WriteError(Loc.GetString("freeze-cmds-err-not-found", ("username", username)));
                continue;
            }

            if (session.AttachedEntity is { } uid)
            {
                continue;
            }

            if (_entity.RemoveComponent<AdminFrozenComponent>(uid))
            {
                shell.WriteLine(Loc.GetString("cmd-unfreeze-success", ("username", username)));
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-unfreeze-err-not-frozen", ("username", username)));
            }
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "username");
    }
}
