using System.Linq;
using Content.Server._Goobstation.StationEvents.Components;
using Content.Server._Goobstation.StationEvents;
using Content.Server.Administration;
using Content.Shared._Goobstation.StationEvents;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.VarEdit)]
public sealed class GameDirectorCommand : IConsoleCommand
{
    public string Command => "gamedirector";
    public string Description => "Gets the entity UID of the Game Director if one is present";
    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var enumerator = entityManager.AllEntityQueryEnumerator<GameDirectorComponent>();
        while (enumerator.MoveNext(out var ent, out _))
        {
            shell.WriteLine($"Game Director: {ent}");
        }
    }
}

[AdminCommand(AdminFlags.VarEdit)]
public sealed class GameDirectorForceStoryCommand : IConsoleCommand
{
    public string Command => "gamedirector:forcestory";
    public string Description => "Forces the game director to switch to a new story";
    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var gameDirector = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GameDirectorSystem>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        if (args.Length < 1)
        {
            shell.WriteLine("Specify a story prototype to force");
            return;
        }

        if (!prototypeManager.TryIndex<StoryPrototype>(args[0], out var story))
        {
            shell.WriteLine($"{args[0]} is not a story prototype");
            return;
        }

        var enumerator = entityManager.AllEntityQueryEnumerator<GameDirectorComponent>();
        while (enumerator.MoveNext(out var ent, out var comp))
        {
            gameDirector.SwitchStory(comp, story, gameDirector.CountActivePlayers());
            shell.WriteLine($"Game Director {ent} switched to {args[0]}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = IoCManager.Resolve<IPrototypeManager>()
                .EnumeratePrototypes<StoryPrototype>()
                .Select(p => new CompletionOption(p.ID))
                .OrderBy(p => p.Value);

            return CompletionResult.FromHintOptions(options, "Story to force");
        }

        return CompletionResult.Empty;
    }
}


[AdminCommand(AdminFlags.VarEdit)]
public sealed class GameDirectorForceEventCommand : IConsoleCommand
{
    public string Command => "gamedirector:forceevent";
    public string Description => "Forces the game director to start an event";
    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var gameDirector = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GameDirectorSystem>();
        var gameTiming = IoCManager.Resolve<IGameTiming>();

        var enumerator = entityManager.AllEntityQueryEnumerator<GameDirectorComponent>();
        while (enumerator.MoveNext(out var ent, out var comp))
        {
            comp.TimeNextEvent = gameTiming.CurTime;
            shell.WriteLine($"Game Director {ent} forced to start an event");
        }
    }
}

[AdminCommand(AdminFlags.VarEdit)]
public sealed class GameDirectorChaosCommand : IConsoleCommand
{
    public string Command => "gamedirector:chaos";
    public string Description => "Causes the game director to evaluate the current amount of chaos";
    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var gameDirector = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GameDirectorSystem>();

        var enumerator = entityManager.AllEntityQueryEnumerator<GameDirectorComponent>();
        while (enumerator.MoveNext(out var ent, out var comp))
        {
            var chaos = gameDirector.CalculateChaos(ent);
            shell.WriteLine($"Game Director {ent} calculated the following amount of chaos: {chaos}");
        }
    }
}
