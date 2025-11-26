using Content.Shared.Inventory;

namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event raised on an entity that is attempting to use a psionic power.
/// </summary>
/// <value><see cref="CanUsePower"/> returns true if able to use psionic powers, false if not.</value>
[ByRefEvent]
public sealed class PsionicPowerUseAttemptEvent() : IInventoryRelayEvent
{
    public bool CanUsePower = true;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
};
