using Robust.Shared.GameStates;

namespace Content.Shared._DV.Augments;

/// <summary>
/// Marker component that makes a organ/bodypart require augment power to work.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentMechanismComponent : Component;
