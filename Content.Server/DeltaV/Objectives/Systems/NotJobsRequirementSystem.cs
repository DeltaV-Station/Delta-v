using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles checking the job blacklist for this objective.
/// </summary>
public sealed class NotJobsRequirementSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotJobsRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(EntityUid uid, NotJobsRequirementComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        // if player has no job then don't care
        if (!TryComp<JobComponent>(args.MindId, out var job))
            return;
        foreach (string forbidJob in comp.Jobs)
            if (job.Prototype == forbidJob)
                args.Cancelled = true;
    }
}
