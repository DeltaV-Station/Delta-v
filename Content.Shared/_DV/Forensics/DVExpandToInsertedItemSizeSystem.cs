using Content.Shared.Item;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Forensics;

public sealed class DVExpandToInsertedItemSizeSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVExpandToInsertedItemSizeComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<DVExpandToInsertedItemSizeComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
    }

    private void Refresh(Entity<DVExpandToInsertedItemSizeComponent> ent)
    {
        var slot = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ItemSlot);
        if (slot.ContainedEntity is { } contained)
        {
            var item = Comp<ItemComponent>(contained);
            _item.SetSize(ent, item.Size);
            _item.SetShape(ent, item.Shape);
        }
        else
        {
            _item.SetSize(ent, ent.Comp.EmptySize);
            _item.SetShape(ent, null);
        }
    }

    private void OnItemInserted(Entity<DVExpandToInsertedItemSizeComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (ent.Comp.ItemSlot != args.Container.ID)
            return;

        Refresh(ent);
    }

    private void OnItemRemoved(Entity<DVExpandToInsertedItemSizeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (ent.Comp.ItemSlot != args.Container.ID)
            return;

        Refresh(ent);
    }
}
