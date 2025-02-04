using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Psionics;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Server.Terminator.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.StationEvents.Events;

/// <summary>
/// Creates clones of random players to make into selected antags.
/// 90% of the actual antag's work is done by exterminator (rip) since its a reskin.
/// </summary>
public sealed class ParadoxClonerRule : StationEventSystem<ParadoxClonerRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PsionicsSystem _psionics = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly TerminatorSystem _terminator = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParadoxClonerRuleComponent, AntagSelectEntityEvent>(OnSelectEntity);
    }

    private void OnSelectEntity(Entity<ParadoxClonerRuleComponent> ent, ref AntagSelectEntityEvent args)
    {
        if (args.Session?.AttachedEntity is not {} spawner)
            return;

        Log.Debug($"Rule {ToPrettyString(ent)} creating a paradox anomaly using spawner {spawner}");
        if (!TrySpawnParadoxAnomaly(spawner, out var clone))
            return;

        Log.Info($"Created paradox anomaly {ToPrettyString(clone):clone}");
        args.Entity = clone;
    }

    private bool TrySpawnParadoxAnomaly(EntityUid spawner, [NotNullWhen(true)] out EntityUid? clone)
    {
        clone = null;

        // Get a list of potential candidates
        var candidates = new List<(EntityUid, EntityUid, ProtoId<JobPrototype>, HumanoidCharacterProfile)>();
        var query = EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent>();
        while (query.MoveNext(out var uid, out var mindContainer, out var humanoid))
        {
            if (humanoid.LastProfileLoaded is {} profile &&
                mindContainer.Mind is {} mindId &&
                !_role.MindIsAntagonist(mindId) &&
                _role.MindHasRole<JobRoleComponent>(mindId, out var role) &&
                role?.Comp1.JobPrototype is {} job)
            {
                candidates.Add((uid, mindId, job, profile));
            }
        }

        if (candidates.Count == 0)
        {
            Log.Warning("Found no eligible players to paradox clone!");
            return false;
        }

        clone = SpawnParadoxAnomaly(spawner, candidates);
        return true;
    }

    private EntityUid SpawnParadoxAnomaly(EntityUid spawner, List<(EntityUid, EntityUid, ProtoId<JobPrototype>, HumanoidCharacterProfile)> candidates)
    {
        // Select a candidate.
        var (uid, mindId, job, profile) = _random.Pick(candidates);

        // Spawn the clone.
        var coords = Transform(spawner).Coordinates;
        var station = _station.GetOwningStation(uid);
        var spawned = _stationSpawning.SpawnPlayerMob(coords, job, profile, station);

        // Set the kill target to the chosen player
        _terminator.SetTarget(spawned, mindId);

        // guaranteed psionic power
        var psi = EnsureComp<PotentialPsionicComponent>(spawned);
        _psionics.RollPsionics(spawned, psi, false, 100);

        return spawned;
    }
}
