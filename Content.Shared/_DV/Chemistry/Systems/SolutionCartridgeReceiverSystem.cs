using Content.Shared._DV.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Chemistry.Systems;

public sealed class SolutionCartridgeReceiverSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _container = default!;

    [Dependency] private readonly SharedAppearanceSystem _appearance = default!; // TODO: Use this

    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SolutionCartridgeReceiverComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SolutionCartridgeReceiverComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<SolutionCartridgeReceiverComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<SolutionCartridgeReceiverComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
    }

    private void OnComponentInit(EntityUid uid, SolutionCartridgeReceiverComponent receiver, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, SolutionCartridgeReceiverComponent.CartridgeSlotId, receiver.CartridgeSlot);
    }

    private void OnComponentRemove(EntityUid uid, SolutionCartridgeReceiverComponent receiver, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, receiver.CartridgeSlot);
    }

    private bool DrainInto(EntityUid fromEnt, string fromName, EntityUid toEnt, string toName, FixedPoint2 amount)
    {
        if (!TryComp(fromEnt, out SolutionContainerManagerComponent? fromManager))
            return false;

        if (!_container.TryGetSolution((fromEnt, fromManager),
                fromName,
                out var _,
                out var fromSolution))
            return false;

        if (!TryComp(toEnt, out SolutionContainerManagerComponent? toManager))
            return false;

        if (!_container.TryGetSolution((toEnt, toManager),
                toName,
                out var toSolutionEnt,
                out var _))
            return false;

        if (!_container.TryTransferSolution(toSolutionEnt.Value, fromSolution, amount))
            return false;

        return true;
    }

    private void OnItemInserted(Entity<SolutionCartridgeReceiverComponent> entity,
        ref EntInsertedIntoContainerMessage args)
    {
        var (uid, receiver) = entity;

        // Drain the newly inserted cartridge into the hypospray
        if (!DrainInto(args.Entity,
                receiver.CartridgeSolution,
                uid,
                receiver.HypospraySolution,
                entity.Comp.MaximumVolume))
        {
            return;
        }

        // TODO: Update appearance
    }

    private void OnItemRemoved(Entity<SolutionCartridgeReceiverComponent> entity,
        ref EntRemovedFromContainerMessage args)
    {
        var (uid, receiver) = entity;

        // Drain the hypospray into the cartridge that's been removed
        if (!DrainInto(uid,
                receiver.HypospraySolution,
                args.Entity,
                receiver.CartridgeSolution,
                entity.Comp.MaximumVolume))
        {
            return;
        }

        // TODO: Update appearance
    }
}
