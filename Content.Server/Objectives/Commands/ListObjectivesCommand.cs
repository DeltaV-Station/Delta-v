using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Objectives.Systems;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Objectives.Commands
{
    [AdminCommand(AdminFlags.Logs)]
    public sealed class ListObjectivesCommand : LocalizedCommands
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IPlayerManager _players = default!;

        public override string Command => "lsobjectives";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            ICommonSession? player;
            if (args.Length > 0)
                _players.TryGetSessionByUsername(args[0], out player);
            else
            {
                // DeltaV - Print everyone's objectives START
                // previously: player = shell.Player
                // implemented like this to make it easier to rip out later
                ListAllObjectives(shell);
                return;
                // DeltaV - Print everyone's objectives END
            }

            if (player == null)
            {
                shell.WriteError(LocalizationManager.GetString("shell-target-player-does-not-exist"));
                return;
            }

            var minds = _entities.System<SharedMindSystem>();
            if (!minds.TryGetMind(player, out var mindId, out var mind))
            {
                shell.WriteError(LocalizationManager.GetString("shell-target-entity-does-not-have-message", ("missing", "mind")));
                return;
            }

            shell.WriteLine($"Objectives for player {player.UserId}:");
            var objectives = mind.Objectives.ToList();
            if (objectives.Count == 0)
            {
                shell.WriteLine("None.");
            }

            var objectivesSystem = _entities.System<SharedObjectivesSystem>();
            for (var i = 0; i < objectives.Count; i++)
            {
                var info = objectivesSystem.GetInfo(objectives[i], mindId, mind);
                if (info == null)
                {
                    shell.WriteLine($"- [{i}] {objectives[i]} - INVALID");
                }
                else
                {

                    var progress = (int) (info.Value.Progress * 100f);
                    shell.WriteLine($"- [{i}] {objectives[i]} ({info.Value.Title}) ({progress}%)");
                }
            }
        }

        // DeltaV - Added function START
        private void ListAllObjectives(IConsoleShell shell)
        {
            var minds = _entities.EntityQueryEnumerator<MindComponent>();
            while (minds.MoveNext(out var mindId, out var mindComp))
            {
                ICommonSession? player = null;
                if (mindComp.OwnedEntity is not null)
                {
                    _players.TryGetSessionByEntity(mindComp.OwnedEntity.Value, out player);
                }

                if (mindComp.Objectives.Count > 0)
                {
                    _entities.TryGetComponent<MetaDataComponent>(mindComp.OwnedEntity, out var metaData);
                    shell.WriteMarkup($"\n[bold]{metaData?.EntityName}[/bold] ({player?.Name ?? "No player attached?"})");

                    var objectivesSystem = _entities.System<SharedObjectivesSystem>();
                    var objectives = mindComp.Objectives;

                    for (var i = 0; i < objectives.Count; i++)
                    {
                        var info = objectivesSystem.GetInfo(objectives[i], mindId);
                        if (info == null)
                        {
                            shell.WriteLine($"- [{i}] {objectives[i]} - INVALID");
                        }
                        else
                        {

                            var progress = (int) (info.Value.Progress * 100f);
                            shell.WriteLine($"- [{i}] {objectives[i]} ({info.Value.Title}) ({progress}%)");
                        }
                    }
                }
            }
        }
        // DeltaV - Added function END

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), LocalizationManager.GetString("shell-argument-username-hint"));
            }

            return CompletionResult.Empty;
        }
    }
}
