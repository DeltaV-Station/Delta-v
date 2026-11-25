using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Devour;

/// <summary>
/// Used to mark an entity as being unable to self-revive (e.g preventing Changelings from using their stasis)
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreventSelfRevivalComponent : Component;
