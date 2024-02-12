using Content.Server.DeltaV.EvilTwin.Components;
using Content.Server.DetailExaminable;
using Content.Server.GenericAntag;
using Content.Server.Psionics;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Server.Terminator.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.DeltaV.EvilTwin.Systems;

/// <summary>
/// 90% of the work is done by exterminator since its a reskin.
/// All the logic here is spawning since thats tricky.
/// </summary>
public sealed class EvilTwinSystem : EntitySystem
{
    [Dependency] private readonly GenericAntagSystem _genericAntag = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly PsionicsSystem _psionics = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly TerminatorSystem _terminator = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EvilTwinSpawnerComponent, PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(Entity<EvilTwinSpawnerComponent> ent, ref PlayerAttachedEvent args)
    {
        if (!_mind.TryGetMind(args.Player, out var mindId, out var mind))
            return;

        if (!TrySpawnEvilTwin(ent.Comp.Rule, out var twin))
            return;

        _mind.TransferTo(mindId, twin, ghostCheckOverride: true, mind: mind);
        QueueDel(ent);
    }

    private bool TrySpawnEvilTwin(string rule, [NotNullWhen(true)] out EntityUid? twin)
    {
        twin = null;

        // Get a list of potential candidates
        var candidates = new List<(EntityUid, EntityUid, SpeciesPrototype, HumanoidCharacterProfile)>();
        var query = EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent>();
        while (query.MoveNext(out var uid, out var mindContainer, out var humanoid))
        {
            if (humanoid.LastProfileLoaded is not {} profile)
                continue;

            if (!_proto.TryIndex<SpeciesPrototype>(humanoid.Species, out var species))
                continue;

            if (_mind.GetMind(uid, mindContainer) is not {} mindId || !HasComp<JobComponent>(mindId))
                continue;

            if (_role.MindIsAntagonist(mindId))
                continue;

            // TODO: when metempsychosis real skip whoever has Karma

            candidates.Add((uid, mindId, species, profile));
        }

        twin = SpawnEvilTwin(candidates, rule);
        return twin != null;
    }

    private EntityUid? SpawnEvilTwin(List<(EntityUid, EntityUid, SpeciesPrototype, HumanoidCharacterProfile)> candidates, string rule)
    {
        // Select a candidate.
        if (candidates.Count == 0)
            return null;

        var (uid, mindId, species, profile) = _random.Pick(candidates);
        var jobId = Comp<JobComponent>(mindId).Prototype;
        var job = _proto.Index<JobPrototype>(jobId!);

        // Find a suitable spawn point.
        var station = _station.GetOwningStation(uid);
        var latejoins = new List<EntityUid>();
        var query = EntityQueryEnumerator<SpawnPointComponent>();
        while (query.MoveNext(out var spawnUid, out var spawnPoint))
        {
            if (spawnPoint.SpawnType != SpawnPointType.LateJoin)
                continue;

            if (_station.GetOwningStation(spawnUid) == station)
                latejoins.Add(spawnUid);
        }

        if (latejoins.Count == 0)
            return null;

        // Spawn the twin.
        var destination = Transform(_random.Pick(latejoins)).Coordinates;
        var spawned = Spawn(species.Prototype, destination);

        // Set the kill target to the chosen player
        _terminator.SetTarget(spawned, mindId);
        _genericAntag.MakeAntag(spawned, rule);

        //////////////////////////
        //    /!\ WARNING /!\   //
        // MAJOR SHITCODE BELOW //
        //    /!\ WARNING /!\   //
        //////////////////////////

        // Copy the details.
        _humanoid.LoadProfile(spawned, profile);
        _metaData.SetEntityName(spawned, Name(uid));

        if (TryComp<DetailExaminableComponent>(uid, out var detail))
        {
            var detailCopy = EnsureComp<DetailExaminableComponent>(spawned);
            detailCopy.Content = detail.Content;
        }

        if (job.StartingGear != null && _proto.TryIndex<StartingGearPrototype>(job.StartingGear, out var gear))
        {
            _stationSpawning.EquipStartingGear(spawned, gear, profile);
            _stationSpawning.EquipIdCard(spawned,
                profile.Name,
                job,
                station);
        }

        foreach (var special in job.Special)
        {
            special.AfterEquip(spawned);
        }

        var psi = EnsureComp<PotentialPsionicComponent>(spawned);
        _psionics.RollPsionics(spawned, psi, false, 100);

        return spawned;
    }
}
