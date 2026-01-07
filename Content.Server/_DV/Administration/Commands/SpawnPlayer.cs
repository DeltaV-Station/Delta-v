using System.Linq;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Server.Traits;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SpawnPlayer : LocalizedEntityCommands
{

    [Dependency] private readonly IEntitySystemManager _entitySys = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override string Command => "spawnplayer";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var mindSystem = _entityManager.System<SharedMindSystem>();
        HumanoidCharacterProfile character;

        if (args.Length < 1)
        {
            shell.WriteError(Loc.GetString("cmd-spawnplayer-error-args"));
            shell.WriteError(Loc.GetString("cmd-spawnplayer-help")); // print usage
            return;
        }

        // Parse the player
        if (!_playerManager.TryGetSessionByUsername(args[0], out var player))
        {
            shell.WriteError(Loc.GetString("cmd-spawnplayer-error-player", ("player", args[0])));
            return;
        }

        character = (HumanoidCharacterProfile) _prefs.GetPreferences(player.UserId).SelectedCharacter;

        // Parse the job_id
        var jobName = args.Length > 1 ? args[1] : "Passenger"; // just default to passenger
        var jobExists = _prototypeManager.TryIndex<JobPrototype>(jobName, out var jobProto);

        if (!jobExists)
        {
            shell.WriteError(Loc.GetString("cmd-spawnplayer-error-job"));
            jobName = "Passenger"; // and if they fuck up, just default to passenger again
        }

        var coordinates = shell.Player != null && shell.Player.AttachedEntity != null
            ? _entityManager.GetComponent<TransformComponent>(shell.Player.AttachedEntity.Value).Coordinates
            : _entitySys.GetEntitySystem<GameTicker>().GetObserverSpawnPoint();

        if (player.AttachedEntity == null ||
            !mindSystem.TryGetMind(player.AttachedEntity.Value, out var mindId, out var mind))
            return;

        var mobUid = _entityManager.System<StationSpawningSystem>().SpawnPlayerMob(coordinates, profile: character, entity: null, job: jobName, station: null);

        // Parse out if we should transfer the mind, and do it if true. Will force even if player has an active 'non-ghost' character.
        if (args.Length > 2 && bool.TryParse(args[2], out bool transferMind) && transferMind)
        {
            mindSystem.TransferTo(mindId, mobUid, transferMind);
        }
        else
            shell.WriteLine(Loc.GetString("cmd-spawnplayer-info-mind-not-transferred", ("entId", mobUid.ToString()), ("player", args[0])));

        shell.WriteLine(Loc.GetString("cmd-spawnplayer-complete"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-spawnplayer-arg-player"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<JobPrototype>(),
                Loc.GetString("cmd-spawnplayer-arg-job"));
        }

        if (args.Length == 3)
        {
            return CompletionResult.FromHintOptions(["true", "false"],
                Loc.GetString("cmd-spawnplayer-arg-transfer-mind"));
        }

        return CompletionResult.Empty;
    }
}
