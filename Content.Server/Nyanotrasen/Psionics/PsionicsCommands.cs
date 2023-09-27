using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mobs.Components;
using Robust.Shared.Console;
using Robust.Server.GameObjects;

namespace Content.Server.Psionics;

[AdminCommand(AdminFlags.Logs)]
public sealed class ListPsionicsCommand : IConsoleCommand
{
    public string Command => "lspsionics";
    public string Description => Loc.GetString("command-lspsionic-description");
    public string Help => Loc.GetString("command-lspsionic-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        foreach (var (actor, mob, psionic, meta) in entMan.EntityQuery<ActorComponent, MobStateComponent, PsionicComponent, MetaDataComponent>())
            // filter out xenos, etc, with innate telepathy
            if (psionic.PsionicAbility?.ToString() != null)
                //This should work. Don't know what the gimp is with Loc.GetString().
                shell.WriteLine(meta.EntityName + " (" + meta.Owner + ") - " + actor.PlayerSession.Name + " - " + "Some Psionic Ability"); ///Loc.GetString(psionic.PsionicAbility?.ToString()));
    }
}
