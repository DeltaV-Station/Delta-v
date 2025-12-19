using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Roles.Jobs;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Systems;

public sealed class RerollAfterCompletionSystem : EntitySystem
{
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly JobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private readonly HashSet<RerollAfterCompletionComponent> _objectivesToAdd = new();

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
            if (component.Rerolled) // If already rerolled, skip.
                continue;

            if (!HasComp<ObjectiveComponent>(uid))
                continue; // If the entity doesn't have an ObjectiveComponent, skip.

            if (!TryComp<MindComponent>(component.MindUid, out var mind))
                continue; // If the mind component is missing, skip.

            // Check that this objective has been completed.
            if (!_objectives.IsCompleted(uid, new(component.MindUid, mind)))
                continue;

            // Destroy this commponent as it is no longer needed, and this will speed up the next check.
            RemCompDeferred<RerollAfterCompletionComponent>(uid);

            component.Rerolled = true;

            // I'd be a lot happier if I could do all the rerolling here
            // But creating the new objective causes the Query to freak out
            // And I need the objective to do everything else.
            _objectivesToAdd.Add(component);
        }

        foreach (var component in _objectivesToAdd)
        {
            var mind = component.MindUid;
            if (!TryComp<MindComponent>(mind, out var mindComponent))
                continue;
            // Create a new objective with the specified prototype.
            if (_objectives.TryCreateObjective(mind, mindComponent, component.RerollObjectivePrototype) is not { } newObjUid)
                continue;
            if (component.RerollObjectiveMessage is null)
                continue;

            var bodyUid = mindComponent.CurrentEntity ?? component.MindUid;

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
            _mind.AddObjective(mind, mindComponent, newObjUid);
        }
    }

    private void OnObjectiveAfterAssign(EntityUid uid, RerollAfterCompletionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        // If the objective is assigned, we can set the mind UID.
        if (args.Mind != null)
            comp.MindUid = args.MindId;
    }
}
