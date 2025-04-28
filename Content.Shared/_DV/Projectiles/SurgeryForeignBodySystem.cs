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

    private void OnForeignBodySurgeryValid(Entity<SurgeryForeignBodyConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp<BodyPartForeignBodyContainerComponent>(args.Part, out var part))
        {
            args.Cancelled = true;
            return;
        }

        if (!_container.TryGetContainer(args.Part, part.ContainerName, out var contents))
        {
            args.Cancelled = true;
            return;
        }

        args.Cancelled = contents.ContainedEntities.Count == 0;
    }

    private void OnForeignBodyStep(Entity<SurgeryRemoveForeignBodyStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<BodyPartForeignBodyContainerComponent>(args.Part, out var part))
            return;

        if (!_container.TryGetContainer(args.Part, part.ContainerName, out var contents))
            return;

        if (contents.ContainedEntities.Count == 0)
            return;

        RemComp<ForeignBodyActivelyEmbeddedComponent>(contents.ContainedEntities[0]);
        _container.Remove(contents.ContainedEntities[0], contents);
    }

    private void OnForeignBodyCheck(Entity<SurgeryRemoveForeignBodyStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<BodyPartForeignBodyContainerComponent>(args.Part, out var part))
        {
            args.Cancelled = false;
            return;
        }

        if (!_container.TryGetContainer(args.Part, part.ContainerName, out var contents))
        {
            args.Cancelled = false;
            return;
        }

        args.Cancelled = contents.ContainedEntities.Count != 0;
    }
}
