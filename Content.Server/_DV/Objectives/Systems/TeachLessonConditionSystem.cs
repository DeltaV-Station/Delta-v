using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles teach a lesson condition logic, does not assign target.
/// </summary>
public sealed class TeachLessonConditionSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    // TODO: subscribe by ref at some point in the future
    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        // Get the mind of the entity that just died (if it had one)
        // Uses OriginalMind so if someone ghosts or otherwise loses control of a mob, you can still greentext
        if (!TryComp<MindContainerComponent>(args.Target, out var mc) || mc.OriginalMind is not {} mindId)
            return;

        // Get all TeachLessonConditionComponent entities
        var query = EntityQueryEnumerator<TeachLessonConditionComponent, TargetObjectiveComponent>();

        while (query.MoveNext(out var uid, out _, out var targetObjective))
        {
            // Check if this objective's target matches the entity that died
            if (targetObjective.Target != mindId)
                continue;

            _codeCondition.SetCompleted(uid);
        }
    }
}
