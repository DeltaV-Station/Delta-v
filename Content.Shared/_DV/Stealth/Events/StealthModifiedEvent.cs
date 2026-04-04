using Content.Shared.Inventory;

namespace Content.Shared._DV.Stealth;

[ByRefEvent]
public record struct StealthModifiedEvent(bool? Enabled = null, float? MaxVisibility = null, float? MinVisibility = null);
