using Content.Server._DV.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Filters;

namespace Content.Server._DV.Mind.Filters;

/// <summary>
/// A mind filter that removes minds if they are immune from being targets.
/// </summary>
public sealed partial class ObjectiveImmuneFilter : MindFilter
{
    protected override bool ShouldRemove(Entity<MindComponent> mind, EntityUid? excluded, IEntityManager entMan, SharedMindSystem mindSys)
    {
        // Check the mind first
        if (entMan.HasComponent<TargetObjectiveImmuneComponent>(mind))
            return true;

        // Check the attached entity, just in case
        if (mind.Comp.OwnedEntity.HasValue &&
            entMan.HasComponent<TargetObjectiveImmuneComponent>(mind.Comp.CurrentEntity))
            return true;

        return false;
    }
}
