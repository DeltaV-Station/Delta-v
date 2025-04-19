using Content.Server._DV.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles checking the department blacklist for this objective.
/// </summary>
public sealed class NotDepartmentRequirementSystem : EntitySystem
{
    [Dependency] private readonly SharedJobSystem _job = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotDepartmentRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(Entity<NotDepartmentRequirementComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled ||
            !_job.MindTryGetJob(args.MindId, out var job) ||
            !_job.TryGetPrimaryDepartment(job.ID, out var primary))
        {
            return;
        }

        if (primary.ID == ent.Comp.Department)
            args.Cancelled = true;
    }
}
