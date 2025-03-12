using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.Shared._DV.SmartFridge;

public sealed class SmartFridgeSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartFridgeComponent, MapInitEvent>(OnFridgeInit);
        SubscribeLocalEvent<SmartFridgeComponent, ComponentShutdown>(OnFridgeShutdown);

        SubscribeLocalEvent<SmartFridgeComponent, ItemSlotInsertAttemptEvent>(OnAttemptInsert);

        SubscribeLocalEvent<SmartFridgeComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<SmartFridgeComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        Subs.BuiEvents<SmartFridgeComponent>(SmartFridgeUiKey.Key,
            sub =>
            {
                sub.Event<SmartFridgeDispenseItemMessage>(OnDispenseItem);
            });
    }

    private void OnFridgeInit(Entity<SmartFridgeComponent> ent, ref MapInitEvent args)
    {
        _itemSlots.AddItemSlot(ent, SmartFridgeComponent.InsertionSlotId, ent.Comp.InsertionSlot);
    }

    private void OnFridgeShutdown(Entity<SmartFridgeComponent> ent, ref ComponentShutdown args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.InsertionSlot);
    }

    private void OnItemInserted(Entity<SmartFridgeComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.Container, out var container))
            return;

        _container.Insert(args.Entity, container);
        var key = new SmartFridgeEntry(Identity.Name(args.Entity, EntityManager));
        if (!ent.Comp.Entries.Contains(key))
            ent.Comp.Entries.Add(key);
        ent.Comp.ContainedEntries.TryAdd(key, new());
        var entries = ent.Comp.ContainedEntries[key];
        if (!entries.Contains(GetNetEntity(args.Entity)))
            entries.Add(GetNetEntity(args.Entity));
        Dirty(ent);
    }

    private void OnItemRemoved(Entity<SmartFridgeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        var key = new SmartFridgeEntry(Identity.Name(args.Entity, EntityManager));

        if (ent.Comp.ContainedEntries.TryGetValue(key, out var contained))
        {
            contained.Remove(GetNetEntity(args.Entity));
        }

        Dirty(ent);
    }

    private void OnAttemptInsert(Entity<SmartFridgeComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.User is not {} user)
            return;

        args.Cancelled = !Allowed(ent, user);
    }

    private bool Allowed(Entity<SmartFridgeComponent> machine, EntityUid user)
    {
        if (_accessReader.IsAllowed(user, machine))
            return true;

        _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-access-denied"), machine, user);
        _audio.PlayPredicted(machine.Comp.SoundDeny, machine, user);
        return false;
    }

    private void OnDispenseItem(Entity<SmartFridgeComponent> ent, ref SmartFridgeDispenseItemMessage args)
    {
        if (!Allowed(ent, args.Actor))
            return;

        if (!ent.Comp.ContainedEntries.TryGetValue(args.Entry, out var contained))
        {
            _audio.PlayPredicted(ent.Comp.SoundDeny, ent, args.Actor);
            _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-unknown-entry"), ent, args.Actor);
            return;
        }

        foreach (var item in contained)
        {
            if (!_container.TryRemoveFromContainer(GetEntity(item)))
                continue;

            _audio.PlayPredicted(ent.Comp.SoundVend, ent, args.Actor);
            contained.Remove(item);
            Dirty(ent);
            return;
        }

        _audio.PlayPredicted(ent.Comp.SoundDeny, ent, args.Actor);
        _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-out-of-stock"), ent, args.Actor);
    }
}
