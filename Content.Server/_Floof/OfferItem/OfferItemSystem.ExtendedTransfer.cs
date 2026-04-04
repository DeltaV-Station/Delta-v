using Content.Shared._DV.Carrying;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;

namespace Content.Server._Floof.OfferItem;

// this entire class part belongs to floofstation
public sealed partial class OfferItemSystem
{
    [Dependency] private readonly CarryingSystem _carrying = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public void InitializeTransfers()
    {
        SubscribeLocalEvent<BeingCarriedComponent, ItemTransferredEvent>(OnCarryTransfer);
        SubscribeLocalEvent<PullableComponent, ItemTransferredEvent>(OnPulledTransfer);
    }

    private void OnCarryTransfer(Entity<BeingCarriedComponent> ent, ref ItemTransferredEvent args)
    {
        if (args.Handled
            || args.PassedItem == args.RealItem // Means the entity is transferred NOT via carrying
            || args.RealItem is not { Valid: true } carried
            || ent.Comp.Carrier is not {Valid: true} oldCarrier)
            return;

        _carrying.DropCarried(oldCarrier, ent);
        args.Handled = _carrying.TryCarry(args.Target, carried);
    }

    private void OnPulledTransfer(Entity<PullableComponent> ent, ref ItemTransferredEvent args)
    {
        if (args.Handled
            || args.PassedItem == args.RealItem // Means the entity is transferred NOT via pulling
            || args.RealItem is not { Valid: true } pulled
            || ent.Comp.Puller is not {Valid: true} oldPuller)
            return;

        _pulling.TryStopPull(oldPuller, ent);
        args.Handled = _pulling.TryStartPull(args.Target, ent, null, ent.Comp);
    }

    private bool TryHandleExtendedTransfer(EntityUid user, EntityUid target, EntityUid offeredItem, EntityUid realItem)
    {
        var ev = new ItemTransferredEvent
        {
            User = user,
            Target = target,
            PassedItem = offeredItem,
            RealItem = realItem,
        };
        RaiseLocalEvent(realItem, ref ev);
        return ev.Handled;
    }
}


// Floofstation section
/// <summary>
///     Raised on the entity that was transferred via item offering.
/// </summary>
[ByRefEvent]
public sealed class ItemTransferredEvent : HandledEntityEventArgs
{
    public EntityUid User;
    public EntityUid Target;

    /// <summary>
    ///     The actual item being passed around. Can be a virtual item.
    /// </summary>
    public EntityUid PassedItem;
    /// <summary>
    ///     If <see cref="PassedItem"/> is a virtual item, this field contains the real item that was transferred.
    /// </summary>
    public EntityUid? RealItem;
}
// Floofstation section end
