using Content.Server.Administration;
using Content.Shared._DV.CCVars;
using Content.Shared.Administration;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._DV.Chat;

[AdminCommand(AdminFlags.Admin)]
public sealed class SetAntagOOCCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;

    public override string Command => "setantagooc";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 0), ("upper", 1)));
            return;
        }

        var antagOoc = _configManager.GetCVar(DCCVars.AntagOOCEnabled);

        if (args.Length == 0)
        {
            antagOoc = !antagOoc;
        }

        if (args.Length == 1 && !bool.TryParse(args[0], out antagOoc))
        {
            shell.WriteError(Loc.GetString("shell-invalid-bool"));
            return;
        }

        _configManager.SetCVar(DCCVars.AntagOOCEnabled, antagOoc);

        shell.WriteLine(Loc.GetString(antagOoc ? "cmd-setantagooc-antagooc-enabled" : "cmd-setantagooc-antagooc-disabled"));
    }
}
