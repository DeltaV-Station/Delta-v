using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// This entity will not be candidate for a paradox clone.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NoParadoxCloneComponent : Component;
