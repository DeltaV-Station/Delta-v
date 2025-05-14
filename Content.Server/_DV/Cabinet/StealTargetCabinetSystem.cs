using Content.Shared.Objectives.Components;
using Robust.Shared.Containers;

namespace Content.Server._DV.Cabinet;

/// <summary>
/// Handles container events for <see cref="StealTargetCabinetComponent"/>.
/// </summary>
public sealed class StealTargetCabinetSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StealTargetCabinetComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<StealTargetCabinetComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }

    private void OnEntInserted(Entity<StealTargetCabinetComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (TryComp<StealTargetComponent>(args.Entity, out var target))
            EnsureComp<StealTargetComponent>(ent).StealGroup = target.StealGroup;
    }

    private void OnEntRemoved(Entity<StealTargetCabinetComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        RemComp<StealTargetComponent>(ent);
    }
}
