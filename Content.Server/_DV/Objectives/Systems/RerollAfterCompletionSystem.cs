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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RerollAfterCompletionComponent, ObjectiveAfterAssignEvent>(OnObjectiveAfterAssign);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        List<EntityUid> toRemove = new();

        var query = EntityQueryEnumerator<RerollAfterCompletionComponent>();

        // Apparently, I need to collect these all before I generate new objectives.
        List<Entity<RerollAfterCompletionComponent>> rerollers = new();
        while (query.MoveNext(out var uid, out var component))
            rerollers.Add(new(uid, component));

        foreach (var (uid, component) in rerollers)
        {
            if (component.Rerolled) // If already rerolled, skip.
                continue;

            if (!TryComp<ObjectiveComponent>(uid, out var objective))
                continue; // If the entity doesn't have an ObjectiveComponent, skip.

            if (!TryComp<MindComponent>(component.MindUid, out var mind))
                continue; // If the mind component is missing, skip.

            // Check that this objective has been completed.
            if (!_objectives.IsCompleted(uid, new(component.MindUid, mind)))
                continue;

            if (component.RerollObjectivePrototype is null) // Ensure prototype is set. Shouldn't happen most of the time.
                continue;

            component.Rerolled = true;

            // Create a new objective with the specified prototype.
            var newObjUid = _objectives.TryCreateObjective(component.MindUid, mind, component.RerollObjectivePrototype);
            if (newObjUid is not null && component.RerollObjectiveMessage is not null)
            {
                _mind.AddObjective(component.MindUid, mind, newObjUid.Value);
                // Check if this has a target component, and if so, get it's name for Localization.
                if (TryComp<TargetObjectiveComponent>(newObjUid, out var targetComp) && TryComp<MindComponent>(targetComp.Target, out var targetMindComp))
                {
                    var newTarget = targetComp.Target;
                    var targetJob = _job.MindTryGetJobName(targetComp.Target);

                    _popup.PopupEntity(Loc.GetString(component.RerollObjectiveMessage, ("targetName", newTarget), ("job", targetJob)), newObjUid.Value, newObjUid.Value, PopupType.Large);
                }
                else
                {
                    _popup.PopupEntity(Loc.GetString(component.RerollObjectiveMessage), newObjUid.Value, newObjUid.Value, PopupType.Large);
                }
            }


            // Destroy this commponent as it is no longer needed, and this will speed up the next check.
            toRemove.Add(uid);
        }

        foreach (var uid in toRemove)
            RemCompDeferred<RerollAfterCompletionComponent>(uid);
    }

    private void OnObjectiveAfterAssign(EntityUid uid, RerollAfterCompletionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        // If the objective is assigned, we can set the mind UID.
        if (args.Mind != null)
            comp.MindUid = args.MindId;
    }
}
