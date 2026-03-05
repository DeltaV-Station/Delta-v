using Content.Server._DV.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Components;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles checking the job blacklist for this objective.
/// </summary>
public sealed class NotJobsRequirementSystem : EntitySystem
{
    private EntityQuery<MindRoleComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<MindRoleComponent>();

        SubscribeLocalEvent<NotJobsRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(Entity<NotJobsRequirementComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        foreach (var forbidJob in ent.Comp.Jobs)
        {
            foreach (var roleId in args.Mind.MindRoleContainer.ContainedEntities)
            {
                if (_query.CompOrNull(roleId)?.JobPrototype == forbidJob)
                    args.Cancelled = true;
            }
        }
    }
}
