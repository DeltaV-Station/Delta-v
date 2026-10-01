using Content.Shared._DV.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;

namespace Content.Shared._DV.Clothing.Systems;

public abstract class LanyardSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public const string ContainerName = "paper_label";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanyardComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LanyardComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(Entity<LanyardComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent, ContainerName, ent.Comp.LabelSlot);
    }

    private void OnComponentRemove(Entity<LanyardComponent> ent, ref ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.LabelSlot);
    }
}
