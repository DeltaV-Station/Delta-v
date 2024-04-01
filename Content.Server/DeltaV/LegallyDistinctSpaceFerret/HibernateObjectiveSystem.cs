using Content.Shared.DeltaV.SpaceFerret;
using Content.Shared.Objectives.Components;

namespace Content.Server.DeltaV.LegallyDistinctSpaceFerret;

public sealed class HibernateObjectiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HibernateConditionComponent, ObjectiveGetProgressEvent>(OnHibernateGetProgress);
    }

    private static void OnHibernateGetProgress(EntityUid uid, HibernateConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = comp.Hibernated ? 1.0f : 0.0f;
    }
}
