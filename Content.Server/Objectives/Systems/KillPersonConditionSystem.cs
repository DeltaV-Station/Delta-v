using System.Linq;
using Content.Server.Objectives.Components;
using Content.Server.Revolutionary.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.CCVar;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles; // DeltaV
using Content.Shared.Roles.Jobs; // DeltaV
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes; // DeltaV
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles kill person condition logic and picking random kill targets.
/// </summary>
public sealed class KillPersonConditionSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!; // DeltaV
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!; // DeltaV
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KillPersonConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<PickRandomPersonComponent, ObjectiveAssignedEvent>(OnPersonAssigned);
        SubscribeLocalEvent<PickRandomHeadComponent, ObjectiveAssignedEvent>(OnHeadAssigned);
    }

    private void OnGetProgress(EntityUid uid, KillPersonConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(uid, out var target))
            return;

        args.Progress = GetProgress(target.Value, comp.RequireDead);
    }

    private void OnPersonAssigned(Entity<PickRandomPersonComponent> ent, ref ObjectiveAssignedEvent args)
    {
        AssignRandomTarget(ent, ref args, _ => true, ent.Comp.OnlyChoosableJobs); // DeltaV: pass onlyJobs
    }

    private void OnHeadAssigned(Entity<PickRandomHeadComponent> ent, ref ObjectiveAssignedEvent args)
    {
        AssignRandomTarget(ent, ref args, mindId =>
            TryComp<MindComponent>(mindId, out var mind) &&
            mind.OwnedEntity is { } ownedEnt &&
            HasComp<CommandStaffComponent>(ownedEnt));
    }

    // DeltaV: added onlyJobs
    private void AssignRandomTarget(EntityUid uid, ref ObjectiveAssignedEvent args, Predicate<EntityUid> filter, bool onlyJobs = true, bool fallbackToAny = true)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        // Get all alive humans, filter out any with TargetObjectiveImmuneComponent
        var allHumans = _mind.GetAliveHumans(args.MindId)
            .Where(mindId =>
            {
                if (!TryComp<MindComponent>(mindId, out var mindComp) || mindComp.OwnedEntity == null)
                    return false;
                return !HasComp<TargetObjectiveImmuneComponent>(mindComp.OwnedEntity.Value);
            })
            .ToList();

        // Begin DeltaV Additions: Only target people with jobs
        if (onlyJobs)
        {
            allHumans.RemoveAll(mindId => !(
                _role.MindHasRole<JobRoleComponent>((mindId.Owner, mindId.Comp), out var role) &&
                role?.Comp1.JobPrototype is {} jobId &&
                _proto.Index(jobId).SetPreference));
        }
        // End DeltaV Additions

        // Can't have multiple objectives to kill the same person
        foreach (var objective in args.Mind.Objectives)
        {
            if (HasComp<KillPersonConditionComponent>(objective) && TryComp<TargetObjectiveComponent>(objective, out var kill))
            {
                allHumans.RemoveAll(x => x.Owner == kill.Target);
            }
        }

        // Filter out targets based on the filter
        var filteredHumans = allHumans.Where(mind => filter(mind)).ToList();

        // There's no humans and we can't fall back to any other target
        if (filteredHumans.Count == 0 && !fallbackToAny)
        {
            args.Cancelled = true;
            return;
        }

        // Pick between humans matching our filter or fall back to all humans alive
        var selectedHumans = filteredHumans.Count > 0 ? filteredHumans : allHumans;

        // Still no valid targets even after the fallback
        if (selectedHumans.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        _target.SetTarget(uid, _random.Pick(selectedHumans), target);
    }

    private float GetProgress(EntityUid target, bool requireDead)
    {
        // deleted or gibbed or something, counts as dead
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
            return 1f;

        // dead is success
        if (_mind.IsCharacterDeadIc(mind))
            return 1f;

        // if the target has to be dead dead then don't check evac stuff
        if (requireDead)
            return 0f;

        // if evac is disabled then they really do have to be dead
        if (!_config.GetCVar(CCVars.EmergencyShuttleEnabled))
            return 0f;

        // target is escaping so you fail
        if (_emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value))
            return 0f;

        // evac has left without the target, greentext since the target is afk in space with a full oxygen tank and coordinates off.
        if (_emergencyShuttle.ShuttlesLeft)
            return 1f;

        // if evac is still here and target hasn't boarded, show 50% to give you an indicator that you are doing good
        return _emergencyShuttle.EmergencyShuttleArrived ? 0.5f : 0f;
    }
}
