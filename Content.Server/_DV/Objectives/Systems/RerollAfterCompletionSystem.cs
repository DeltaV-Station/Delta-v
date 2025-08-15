using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Roles.Jobs;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;

namespace Content.Server._DV.Objectives.Systems;

public sealed class RerollAfterCompletionSystem : EntitySystem
{
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly JobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private readonly HashSet<(EntityUid mind, MindComponent mindComponent, EntityUid objective)> _objectivesToAdd = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RerollAfterCompletionComponent, ObjectiveAfterAssignEvent>(OnObjectiveAfterAssign);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _objectivesToAdd.Clear();
        var query = EntityQueryEnumerator<RerollAfterCompletionComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            // Destroy this commponent as it is no longer needed, and this will speed up the next check.
            RemCompDeferred<RerollAfterCompletionComponent>(uid);

            if (component.Rerolled) // If already rerolled, skip.
                continue;

            if (!HasComp<ObjectiveComponent>(uid))
                continue; // If the entity doesn't have an ObjectiveComponent, skip.

            if (!TryComp<MindComponent>(component.MindUid, out var mind))
                continue; // If the mind component is missing, skip.

            // Check that this objective has been completed.
            if (!_objectives.IsCompleted(uid, new(component.MindUid, mind)))
                continue;

            component.Rerolled = true;

            var bodyUid = mind.CurrentEntity ?? component.MindUid;

            // Create a new objective with the specified prototype.
            if (_objectives.TryCreateObjective(component.MindUid, mind, component.RerollObjectivePrototype) is not { } newObjUid)
                continue;
            
            _objectivesToAdd.Add((component.MindUid, mind, newObjUid));
            if (component.RerollObjectiveMessage is null)
                continue;
            
            // Check if this has a target component, and if so, get it's name for Localization.
            if (TryComp<TargetObjectiveComponent>(newObjUid, out var targetComp) && TryComp<MindComponent>(targetComp.Target, out var targetMindComp))
            {
                var newTarget = targetMindComp.CharacterName ?? "Unknown";
                var targetJob = _job.MindTryGetJobName(targetComp.Target);
                _popup.PopupEntity(Loc.GetString(component.RerollObjectiveMessage, ("targetName", newTarget), ("job", targetJob)), bodyUid, bodyUid, PopupType.Large);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString(component.RerollObjectiveMessage), bodyUid, bodyUid, PopupType.Large);
            }
        }

        foreach (var (mind, mindComponent, objective) in _objectivesToAdd)
        {
            _mind.AddObjective(mind, mindComponent, objective);
        }
    }

    private void OnObjectiveAfterAssign(EntityUid uid, RerollAfterCompletionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        // If the objective is assigned, we can set the mind UID.
        if (args.Mind != null)
            comp.MindUid = args.MindId;
    }
}
