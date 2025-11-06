using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.TooShortForUI;

/// <summary>
/// Blocks the use of machine UI on blacklisted or non-whitelisted machines if the entity is not:
/// 1) buckled into something, like a chair or a bed
/// 2) weightless
/// or 3) vaulted on a table.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TooShortForUIComponent : Component
{
    /// <summary>
    /// These entities will *not* be allowed.
    /// If the blacklist is null, blocks all entities.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist = null;
    
    /// <summary>
    /// These entities will be allowed.
    /// Takes priority over Blacklist
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;

    [DataField]
    public LocId? PopupText = "too-short-for-ui-cant-use";
}
