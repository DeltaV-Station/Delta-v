using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Content.Shared.Administration;
using Content.Server.Administration;
using Content.Shared._DV.Tips;
using Robust.Shared.Player;

namespace Content.Server._DV.Tips;

[AdminCommand(AdminFlags.Fun)]
public sealed class ShowTipCommand : LocalizedEntityCommands
{
    public override string Command => "showtip";

    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedTipSystem _tip = default!;

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 =>
                CompletionResult.FromHintOptions(
                    CompletionHelper.SessionNames(players: _player),
                    Loc.GetString("shell-argument-username-hint")),
            2 =>
                CompletionResult.FromHintOptions(
                    CompletionHelper.PrototypeIDs<TipPrototype>(proto: _prototype),
                    Loc.GetString("cmd-showtip-hint-tip")),
            _ => CompletionResult.Empty
        };
    }

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is 0 or > 2)
        {
            shell.WriteError(LocalizationManager.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!_player.TryGetSessionByUsername(args[0], out var player))
        {
            shell.WriteError(LocalizationManager.GetString("shell-target-player-does-not-exist"));
            return;
        }

        if (!_prototype.TryIndex<TipPrototype>(args[1], out var proto))
        {
            shell.WriteError(Loc.GetString($"shell-argument-must-be-prototype",
                ("index", args[1]),
                ("prototype", nameof(TipPrototype))));
            return;
        }

        _tip.ShowTip(player, proto, true);

        shell.WriteLine(Loc.GetString("shell-command-success"));
    }
}
