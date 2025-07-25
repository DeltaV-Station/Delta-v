using Content.Shared.Inventory;
using Content.Shared._DV.Chemistry.Components;

namespace Content.Shared._DV.Chemistry.Systems;

public sealed class SafeSolutionThrowerSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SafeSolutionThrowerComponent, SafeSolutionThrowEvent>(OnSafeSolutionThrowAttempt);
        SubscribeLocalEvent<SafeSolutionThrowerComponent, InventoryRelayedEvent<SafeSolutionThrowEvent>>((e, c, ev) => OnSafeSolutionThrowAttempt(e, c, ev.Args));
    }

    private void OnSafeSolutionThrowAttempt(EntityUid eid, SafeSolutionThrowerComponent component, SafeSolutionThrowEvent args)
    {
        args.SafeThrow = true;
    }
}


public sealed class SafeSolutionThrowEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool SafeThrow;
    public SlotFlags TargetSlots => SlotFlags.HEAD | SlotFlags.MASK | SlotFlags.EYES;
}

