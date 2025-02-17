using Robust.Shared.GameStates;
using Content.Shared.Lock;

namespace Content.Shared._DV.Lock;

/// <summary>
/// This is used for toggleable items that require the entity to have a lock in a certain state.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(LockSystem))]
public sealed partial class ItemToggleRequiresLockComponent : Component
{
    /// <summary>
    /// TRUE: the lock must be locked to toggle the item.
    /// FALSE: the lock must be unlocked to toggle the item.
    /// </summary>
    [DataField]
    public bool RequireLocked;
}
