using Content.Shared.Inventory;

namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event raised on an entity that is attempting to use a psionic power.
/// </summary>
/// <param name="Psionic">The psionic that attempted to use a psionic power.</param>
[ByRefEvent]
public struct PsionicPowerAttemptEvent : IInventoryRelayEvent
{
    public bool CanUsePower;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
};
