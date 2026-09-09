using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Item;
using Content.Shared.NameModifier.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Forensics;

public sealed class DVSeenInsertedItemSystem : EntitySystem
{
    [Dependency] private readonly NameModifierSystem _nameModifier = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVSeenInsertedItemComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DVSeenInsertedItemComponent, RefreshNameModifiersEvent>(OnRefreshModifiers);
        SubscribeLocalEvent<DVSeenInsertedItemComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<DVSeenInsertedItemComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
    }

    private void OnExamined(Entity<DVSeenInsertedItemComponent> ent, ref ExaminedEvent args)
    {
        var slot = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ItemSlot);
        if (slot.ContainedEntity is { } contained)
        {
            args.PushMarkup(Loc.GetString("dv-seen-inserted-item-examined.full", ("container", Identity.Entity(ent, EntityManager, args.Examiner)), ("contained", contained)));
        }
        else
        {
            args.PushMarkup(Loc.GetString("dv-seen-inserted-item-examined.empty", ("container", Identity.Entity(ent, EntityManager, args.Examiner))));
        }
    }

    private void OnRefreshModifiers(Entity<DVSeenInsertedItemComponent> ent, ref RefreshNameModifiersEvent args)
    {
        var slot = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ItemSlot);
        if (slot.ContainedEntity is not { } contained)
            return;

        args.AddModifier("dv-seen-inserted-item-name-modifier", extraArgs: ("contained", contained));
    }

    private void Refresh(Entity<DVSeenInsertedItemComponent> ent)
    {
        _nameModifier.RefreshNameModifiers(ent.Owner);

        var slot = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ItemSlot);
        if (slot.ContainedEntity is { } contained)
        {
            var item = Comp<ItemComponent>(contained);
            _item.CopyVisuals(ent.Owner, item);
        }
        else
        {
            _item.ClearVisuals(ent.Owner);
        }
    }

    private void OnItemInserted(Entity<DVSeenInsertedItemComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ItemSlot || _timing.ApplyingState)
            return;

        Refresh(ent);
    }

    private void OnItemRemoved(Entity<DVSeenInsertedItemComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ItemSlot || _timing.ApplyingState)
            return;

        Refresh(ent);
    }
}
