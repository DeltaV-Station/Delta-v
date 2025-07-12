using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Projectiles;

public sealed class SurgeryForeignBodySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryForeignBodyConditionComponent, SurgeryValidEvent>(OnForeignBodySurgeryValid);

        SubscribeLocalEvent<SurgeryRemoveForeignBodyStepComponent, SurgeryStepEvent>(OnForeignBodyStep);
        SubscribeLocalEvent<SurgeryRemoveForeignBodyStepComponent, SurgeryStepCompleteCheckEvent>(OnForeignBodyCheck);
    }

    private Container? GetContainer(EntityUid uid)
    {
        if (!TryComp<BodyPartForeignBodyContainerComponent>(uid, out var part))
            return null;

        return part.Container;
    }

    private bool HasForeignBodies(EntityUid uid)
    {
        if (GetContainer(uid) is not {} contents)
            return false;

        return contents.ContainedEntities.Count > 0;
    }

    private void OnForeignBodySurgeryValid(Entity<SurgeryForeignBodyConditionComponent> ent, ref SurgeryValidEvent args)
    {
        args.Cancelled = !HasForeignBodies(args.Part);
    }

    private void OnForeignBodyStep(Entity<SurgeryRemoveForeignBodyStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (GetContainer(args.Part) is not {} contents)
            return;

        if (contents.ContainedEntities.Count == 0)
            return;

        RemComp<ForeignBodyActivelyEmbeddedComponent>(contents.ContainedEntities[0]);
        _container.Remove(contents.ContainedEntities[0], contents);
    }

    private void OnForeignBodyCheck(Entity<SurgeryRemoveForeignBodyStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        args.Cancelled = HasForeignBodies(args.Part);
    }
}
