using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Exists for strap entities to apply surgical anesthesia to the patient upon being buckled.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AnesthesiaOnBuckleComponent : Component
{
    /// <summary>
    /// Check whether the buckled entity was anesthetized already.
    /// </summary>
    [DataField]
    public bool HadAnesthesia;
}
