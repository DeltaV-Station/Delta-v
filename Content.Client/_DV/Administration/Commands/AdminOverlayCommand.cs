using Content.Client.Administration.Systems;
using Robust.Shared.Console;

namespace Content.Client._DV.Administration.Commands;

public sealed class AdminOverlayCommand : LocalizedEntityCommands
{
    [Dependency] private readonly AdminSystem _admin = default!;
    public override string Command => "adminoverlay";
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !Boolean.TryParse(args[0], out var newValue))
        {
            shell.WriteLine(Help);
            return;
        }

        if (newValue)
        {
            _admin.AdminOverlayOn();
        }
        else
        {
            _admin.AdminOverlayOff();
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1 ? CompletionResult.FromOptions(CompletionHelper.Booleans) : CompletionResult.Empty;
    }
}
