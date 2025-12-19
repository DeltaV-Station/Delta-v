using System.Linq;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Station;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SpawnCharacter : LocalizedEntityCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySys = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override string Command => "spawncharacter";
    public override string Description => Loc.GetString("cmd-spawncharacter-desc");
    public override string Help => Loc.GetString("cmd-spawncharacter-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not ICommonSession player)
        {
            shell.WriteError(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        var mindSystem = _entityManager.System<SharedMindSystem>();

        var data = player.ContentData();

        if (data?.UserId == null)
        {
            shell.WriteError(Loc.GetString("shell-entity-is-not-mob"));
            return;
        }


        HumanoidCharacterProfile character;

        if (args.Length >= 1)
        {
            var name = args[0]; // Auto-complete adds quotes around the name, so no need to worry about spaces.
            shell.WriteLine(Loc.GetString("loadcharacter-command-fetching", ("name", name)));

            if (!FetchCharacters(data.UserId, out var characters))
            {
                shell.WriteError(Loc.GetString("loadcharacter-command-failed-fetching"));
                return;
            }

            var selectedCharacter = characters.FirstOrDefault(c => c.Name == name);

            if (selectedCharacter == null)
            {
                shell.WriteError(Loc.GetString("loadcharacter-command-failed-fetching"));
                return;
            }

            character = selectedCharacter;
        }
        else
            character = (HumanoidCharacterProfile)_prefs.GetPreferences(data.UserId).SelectedCharacter;

        var jobName = args.Length > 1 ? args[1] : "Passenger"; // just default to passenger
        var jobExists = _prototypeManager.TryIndex<JobPrototype>(jobName, out var jobProto);

        if (!jobExists)
        {
            shell.WriteError(Loc.GetString("cmd-spawncharacter-error-job"));
            jobName = "Passenger"; // and if they fuck up, just default to passenger again
        }


        var coordinates = player.AttachedEntity != null
            ? _entityManager.GetComponent<TransformComponent>(player.AttachedEntity.Value).Coordinates
            : _entitySys.GetEntitySystem<GameTicker>().GetObserverSpawnPoint();

        if (player.AttachedEntity == null ||
            !mindSystem.TryGetMind(player.AttachedEntity.Value, out var mindId, out var mind))
            return;

        var mobUid = _entityManager.System<StationSpawningSystem>().SpawnPlayerMob(coordinates, profile: character, entity: null, job: jobName, station: null);
        mindSystem.TransferTo(mindId, mobUid);

        shell.WriteLine(Loc.GetString("cmd-spawncharacter-complete"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var player = shell.Player as ICommonSession;
            if (player == null)
                return CompletionResult.Empty;

            var data = player.ContentData();
            var mind = data?.Mind;

            if (mind == null || data == null)
                return CompletionResult.Empty;

            return FetchCharacters(data.UserId, out var characters)
                ? CompletionResult.FromHintOptions(characters.Select(c => c.Name), Loc.GetString("cmd-spawncharacter-arg-character"))
                : CompletionResult.Empty;
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<JobPrototype>(),
                Loc.GetString("cmd-spawncharacter-arg-job"));
        }

        return CompletionResult.Empty;
    }

    private bool FetchCharacters(NetUserId player, out HumanoidCharacterProfile[] characters)
    {
        characters = null!;
        if (!_prefs.TryGetCachedPreferences(player, out var prefs))
            return false;

        characters = prefs.Characters
            .Where(kv => kv.Value is HumanoidCharacterProfile)
            .Select(kv => (HumanoidCharacterProfile)kv.Value)
            .ToArray();

        return true;
    }
}
