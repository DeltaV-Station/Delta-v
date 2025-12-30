using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Containers;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Cabinet;

/// <summary>
/// Controls ItemCabinet slot locking and visuals.
/// </summary>
public sealed class ItemCabinetSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly OpenableSystem _openable = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private EntityQuery<ItemCabinetComponent> _cabinetQuery = default!; // DeltaV
    private EntityQuery<ItemSlotsComponent> _slotsQuery = default!; // DeltaV

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _cabinetQuery = GetEntityQuery<ItemCabinetComponent>(); // DeltaV
        _slotsQuery = GetEntityQuery<ItemSlotsComponent>(); // DeltaV

        SubscribeLocalEvent<ItemCabinetComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ItemCabinetComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ItemCabinetComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<ItemCabinetComponent, EntRemovedFromContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<ItemCabinetComponent, OpenableOpenedEvent>(OnOpened);
        SubscribeLocalEvent<ItemCabinetComponent, OpenableClosedEvent>(OnClosed);
    }

    private void OnStartup(Entity<ItemCabinetComponent> ent, ref ComponentStartup args)
    {
        UpdateAppearance(ent);
    }

    private void OnMapInit(Entity<ItemCabinetComponent> ent, ref MapInitEvent args)
    {
        // update at mapinit to avoid copy pasting locked: true and locked: false for each closed/open prototype
        SetSlotLock(ent, _openable.IsClosed(ent, null));
    }

    private void UpdateAppearance(Entity<ItemCabinetComponent> ent)
    {
        _appearance.SetData(ent, ItemCabinetVisuals.ContainsItem, HasItem((ent, ent.Comp)));
    }

    private void OnContainerModified(EntityUid uid, ItemCabinetComponent component, ContainerModifiedMessage args)
    {
        if (args.Container.ID == component.Slot)
            UpdateAppearance((uid, component));
    }

    private void OnOpened(Entity<ItemCabinetComponent> ent, ref OpenableOpenedEvent args)
    {
        SetSlotLock(ent, false);
    }

    private void OnClosed(Entity<ItemCabinetComponent> ent, ref OpenableClosedEvent args)
    {
        SetSlotLock(ent, true);
    }

    /// <summary>
    /// Tries to get the cabinet's item slot.
    /// </summary>
    public bool TryGetSlot(Entity<ItemCabinetComponent?> ent, [NotNullWhen(true)] out ItemSlot? slot) // DeltaV - made component optional
    {
        slot = null;
        // Begin DeltaV Changes: Use queries instead of TryComp
        if (!_cabinetQuery.Resolve(ent, ref ent.Comp))
            return false;

        if (!_slotsQuery.TryComp(ent, out var slots))
            return false;
        // End DeltaV Changes

        return _slots.TryGetSlot(ent, ent.Comp.Slot, out slot, slots);
    }

    /// <summary>
    /// Returns true if the cabinet contains an item.
    /// </summary>
    public bool HasItem(Entity<ItemCabinetComponent?> ent) // DeltaV - made component optional
    {
        return TryGetSlot(ent, out var slot) && slot.HasItem;
    }

    /// <summary>
    /// Lock or unlock the underlying item slot.
    /// </summary>
    public void SetSlotLock(Entity<ItemCabinetComponent> ent, bool closed)
    {
        if (!_slotsQuery.TryComp(ent, out var slots)) // DeltaV - use query
            return;

        if (_slots.TryGetSlot(ent, ent.Comp.Slot, out var slot, slots))
            _slots.SetLock(ent, slot, closed, slots);
    }
}
