using Content.Shared.Inventory;

namespace Content.Shared._DV.Stealth;

[ByRefEvent]
public record struct StealthAddedEvent(EntityUid CloakedEntity) : IInventoryRelayEvent
{
    public EntityUid CloakedEntity = CloakedEntity;

    public SlotFlags TargetSlots => SlotFlags.All;
}
