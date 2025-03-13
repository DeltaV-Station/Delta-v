using Content.Server.Antag;
using Content.Server.Cloning;
using Content.Server.GameTicking.Rules;
using Content.Server.Psionics;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Server.Terminator.Systems;
using Content.Shared.Cloning;
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
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PsionicsSystem _psionics = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
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

        var settings = _proto.Index(ent.Comp.CloningSettings);
        Log.Debug($"Rule {ToPrettyString(ent)} creating a paradox anomaly using spawner {spawner}");
        if (!TrySpawnParadoxAnomaly(spawner, settings, out var clone))
            return;

        Log.Info($"Created paradox anomaly {ToPrettyString(clone):clone}");
        args.Entity = clone;
    }

    private bool TrySpawnParadoxAnomaly(EntityUid spawner, CloningSettingsPrototype settings, [NotNullWhen(true)] out EntityUid? clone)
    {
        clone = null;

        // Get a list of potential candidates
        var candidates = new List<(EntityUid, EntityUid, ProtoId<JobPrototype>)>();
        var query = EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent>();
        while (query.MoveNext(out var uid, out var mindContainer, out var humanoid))
        {
            if (mindContainer.Mind is {} mindId &&
                !_role.MindIsAntagonist(mindId) &&
                _role.MindHasRole<JobRoleComponent>(mindId, out var role) &&
                role?.Comp1.JobPrototype is {} job)
            {
                candidates.Add((uid, mindId, job));
            }
        }

        if (candidates.Count == 0)
        {
            Log.Warning("Found no eligible players to paradox clone!");
            return false;
        }

        // tries20 my beloved
        for (int i = 0; i < 20; i++)
        {
            clone = SpawnParadoxAnomaly(spawner, settings, candidates);
            if (clone != null)
                return true;
        }

        Log.Error("Failed to clone any eligible player!");
        return false;
    }

    private EntityUid? SpawnParadoxAnomaly(EntityUid spawner, CloningSettingsPrototype settings, List<(EntityUid, EntityUid, ProtoId<JobPrototype>)> candidates)
    {
        // Select a candidate.
        var (uid, mindId, job) = _random.Pick(candidates);

        // Spawn the clone.
        var coords = _transform.GetMapCoordinates(spawner);
        var station = _station.GetOwningStation(uid);
        if (!_cloning.TryCloning(uid, coords, settings, out var mob))
            return null;

        // Set the kill target to the chosen player
        var spawned = mob.Value;
        _terminator.SetTarget(spawned, mindId);

        // guaranteed psionic power
        var psi = EnsureComp<PotentialPsionicComponent>(spawned);
        _psionics.RollPsionics(spawned, psi, false, 100);

        return spawned;
    }
}
