using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// This entity cannot be cloned but can still be revived by defibrillators.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UncloneableComponent : Component;
