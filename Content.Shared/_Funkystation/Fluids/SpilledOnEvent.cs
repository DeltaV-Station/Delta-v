using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;

namespace Content.Shared._Funkystation.Fluids;

/// <summary>
/// Raised when a fluid is spilled on an entity
/// </summary>
public sealed class SpilledOnEvent(EntityUid source, Solution solution, SlotFlags slotFlags = SlotFlags.WITHOUT_POCKET, bool ignoreBlockers = false) : EntityEventArgs, IInventoryRelayEvent
{
    public EntityUid Source = source;
    public Solution Solution = solution;
    public bool IgnoreBlockers = ignoreBlockers;

    public SlotFlags TargetSlots => slotFlags;
}
