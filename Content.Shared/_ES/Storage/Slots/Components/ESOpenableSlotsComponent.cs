using Robust.Shared.GameStates;

namespace Content.Shared._ES.Storage.Slots.Components;

/// <summary>
/// A generic sort of cabinet that can be opened, locked, and have items taken out of it.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESOpenableSlotSystem), Other = AccessPermissions.None)]
public sealed partial class ESOpenableSlotsComponent : Component
{
    /// <summary>
    /// Slots affected
    /// </summary>
    [DataField]
    public HashSet<string> Slots = new();
}
