using Content.Shared.Inventory;

/// <summary>
///     Raised on an entity when a surgery is about to be performed, in case a system wants to modify the speed, such as surgical gloves.
/// </summary>
[ByRefEvent]
public record struct SurgerySpeedModifyEvent() : IInventoryRelayEvent
{
    public float Multiplier = 1f;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
