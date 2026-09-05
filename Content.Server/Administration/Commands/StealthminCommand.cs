using Content.Server.Administration.Managers;
using Content.Shared._DV.Administration.Components; // DeltaV - Unorbitable
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Stealth)]
public sealed class StealthminCommand : LocalizedEntityCommands // DeltaV - Unorbitable, LocalizedCommands to LocalizedEntityCommands
{
    [Dependency] private readonly IAdminManager _adminManager = default!;

    public override string Command => "stealthmin";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player == null)
        {
            shell.WriteLine(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        var adminData = _adminManager.GetAdminData(player);

        DebugTools.AssertNotNull(adminData);

        if (!adminData!.Stealth)
        {
            _adminManager.Stealth(player);
            // DeltaV - Unorbitable START
            if (player.AttachedEntity is { } attachedEntity)
                EntityManager.EnsureComponent<UnorbitableComponent>(attachedEntity);
            // DeltaV END
        }
        else
        {
            _adminManager.UnStealth(player);
            // DeltaV - Unorbitable START
            if (player.AttachedEntity is { } attachedEntity)
                EntityManager.RemoveComponent<UnorbitableComponent>(attachedEntity);
            // DeltaV END
        }
    }
}
