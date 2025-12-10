using Content.Shared.Damage;
using Content.Shared.Inventory;

namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event raised on an entity when a noöspheric fry gamerule happens.
/// </summary>
/// <param name="damage">The damage dealt to every entity wearing insulative gear.</param>
/// <param name="fireStacks">The firestacks added to each </param>
[ByRefEvent]
public readonly struct NoosphericFryEvent(DamageSpecifier damage, int fireStacks) : IInventoryRelayEvent
{
    public readonly DamageSpecifier Damage = damage;
    public readonly int FireStacks = fireStacks;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
};
