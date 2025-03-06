using Robust.Shared.GameStates;
using Content.Shared.Inventory;

/// <summary>
///     Raised on an entity when a surgery is about to be performed, in case a system wants to modify the speed, such as surgical gloves.
/// </summary>
[ByRefEvent]
public record struct SurgerySpeedModifyEvent(float Multiplier) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;
}
