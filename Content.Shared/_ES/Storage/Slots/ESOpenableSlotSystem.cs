using Content.Shared._ES.Storage.Slots.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Shared._ES.Storage.Slots;

/// <summary>
/// <see cref="ESOpenableSlotsComponent"/>
/// </summary>
public sealed class ESOpenableSlotSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly OpenableSystem _openable = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESOpenableSlotsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ESOpenableSlotsComponent, OpenableOpenedEvent>(OnOpened);
        SubscribeLocalEvent<ESOpenableSlotsComponent, OpenableClosedEvent>(OnClosed);
    }

    private void OnMapInit(Entity<ESOpenableSlotsComponent> ent, ref MapInitEvent args)
    {
        UpdateSlotsLocked((ent, ent));
    }

    private void OnOpened(Entity<ESOpenableSlotsComponent> ent, ref OpenableOpenedEvent args)
    {
        UpdateSlotsLocked((ent, ent));
    }

    private void OnClosed(Entity<ESOpenableSlotsComponent> ent, ref OpenableClosedEvent args)
    {
        UpdateSlotsLocked((ent, ent));
    }

    private void UpdateSlotsLocked(Entity<ESOpenableSlotsComponent?, ItemSlotsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, logMissing: false))
            return;

        var val = !_openable.IsOpen(ent);
        foreach (var slot in ent.Comp1.Slots)
        {
            _itemSlots.SetLock(ent, slot, val, ent);
        }
    }
}
