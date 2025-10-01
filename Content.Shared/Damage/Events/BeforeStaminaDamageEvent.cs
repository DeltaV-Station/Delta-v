using Content.Shared.Inventory;

namespace Content.Shared.Damage.Events;

/// <summary>
/// Raised before stamina damage is dealt to allow other systems to cancel or modify it.
/// </summary>
[ByRefEvent]
// DeltaV - added FromMelee
public record struct BeforeStaminaDamageEvent(float Value, bool FromMelee, bool Cancelled = false) : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots =>  ~SlotFlags.POCKET;
}
