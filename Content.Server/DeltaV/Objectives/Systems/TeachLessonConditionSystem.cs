using Content.Server.Objectives.Components;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles teach a lesson condition logic, does not assign target.
/// </summary>
public sealed class TeachLessonConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    private List<EntityUid> _wasKilled = new();  //DeltaV Port from EE

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeachLessonConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
    }

    private void OnGetProgress(EntityUid uid, TeachLessonConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(uid, out var target))
            return;

        args.Progress = GetProgress(target.Value);
    }

    private float GetProgress(EntityUid target)
    {
        // deleted or gibbed or something, counts as dead
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null || _mind.IsCharacterDeadIc(mind))
        {
            _wasKilled.Add(target);
            return 1f;
        }
        if (_wasKilled.Contains(target))
        {
            return 1f;
        }
        return 0f;
    }
    // Clear the wasKilled list on round end
    private void OnRoundEnd(RoundRestartCleanupEvent  ev)
    {
        _wasKilled.Clear();
    }
    // DeltaV - end making people only die once from EE
}
