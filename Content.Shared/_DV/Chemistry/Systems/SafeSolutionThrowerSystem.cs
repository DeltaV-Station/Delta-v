using Content.Shared.Inventory;
using Content.Shared._DV.Chemistry.Components;

namespace Content.Shared._DV.Chemistry.Systems;

public sealed class SafeSolutionThrowerSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InventoryComponent, SafeSolutionThrowEvent>(_inventory.RelayEvent);
        Subs.SubscribeWithRelay<SafeSolutionThrowerComponent, SafeSolutionThrowEvent>(OnSafeSolutionThrowAttempt);
    }

    /// <summary>
    /// Call this to check if a player can throw a solution safely.
    /// </summary>
    public bool GetSafeThrow(EntityUid playeruid)
    {
        var safeThrowEvent = new SafeSolutionThrowEvent();
        RaiseLocalEvent(playeruid, ref safeThrowEvent);
        return safeThrowEvent.SafeThrow;
    }

    private void OnSafeSolutionThrowAttempt(Entity<SafeSolutionThrowerComponent> ent, ref SafeSolutionThrowEvent args)
    {
        args.SafeThrow = true;
    }
}

/// <summary>
/// Raised on an entity and its inventory to determine if it can throw spillable objects safely.
/// </summary>
[ByRefEvent]
public record struct SafeSolutionThrowEvent(bool SafeThrow = false) : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.HEAD | SlotFlags.MASK | SlotFlags.EYES;
}
