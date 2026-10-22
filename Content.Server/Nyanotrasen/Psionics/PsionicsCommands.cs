using Content.Server.Administration;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.Administration;
using Content.Shared.Mobs.Components;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Nyanotrasen.Psionics;

[AdminCommand(AdminFlags.Logs)]
public sealed class ListPsionicsCommand : IConsoleCommand
{
    public string Command => "lspsionics";
    public string Description => Loc.GetString("command-lspsionic-description");
    public string Help => Loc.GetString("command-lspsionic-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        foreach (var (actor, mob, psionic, meta) in entMan.EntityQuery<ActorComponent, MobStateComponent, PsionicComponent, MetaDataComponent>()){
            // filter out xenos, etc, with innate telepathy
            if (psionic.PsionicPowersActionEntities.Count == 0)
                return;

            var psiPowerName = "";
            foreach (var power in psionic.PsionicPowersActionEntities)
            {
                psiPowerName += power;
            }

            shell.WriteLine(meta.EntityName + " (" + meta.Owner + ") - " + actor.PlayerSession.Name + Loc.GetString(psiPowerName));
        }
    }
}
