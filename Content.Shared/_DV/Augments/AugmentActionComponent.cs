using Robust.Shared.GameStates;

namespace Content.Shared._DV.Augments;

/// <summary>
///     Component that allows an augment to have actions
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentActionComponent : Component;
