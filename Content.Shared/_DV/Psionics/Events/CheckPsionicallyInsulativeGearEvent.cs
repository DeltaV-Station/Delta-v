using Content.Shared.Inventory;

namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event raised on an entity when their psionics insulation is modified.
/// </summary>
/// <param name="AllowsPsionicUsage">Whether the entity can use psionic abilities.</param>
/// <param name="ShieldsFromPsionics">Whether the entity is shielded from external psionic influence.</param>
[ByRefEvent]
public record struct CheckPsionicallyInsulativeGearEvent(bool? AllowsPsionicUsage, bool? ShieldsFromPsionics) : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
}
