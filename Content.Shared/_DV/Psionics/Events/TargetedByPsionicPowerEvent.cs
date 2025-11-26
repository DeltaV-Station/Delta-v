using Content.Shared.Inventory;

namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event raised on an entity that is being targetted by a psionic power.
/// </summary>
/// <value><see cref="IsShielded"/> returns true if able to use psionic powers, false if not.</value>
[ByRefEvent]
public sealed class TargetedByPsionicPowerEvent() : IInventoryRelayEvent
{
    public bool IsShielded;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
};
