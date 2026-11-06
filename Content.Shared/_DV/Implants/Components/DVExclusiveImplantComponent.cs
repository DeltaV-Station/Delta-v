using Robust.Shared.GameStates;

namespace Content.Shared._DV.Implants.Components;

/// <summary>
/// An implant with this component cannot be added to an entity that has another implant with the same <see cref="ExclusiveKey"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DVExclusiveImplantComponent : Component
{
    /// <summary>
    /// The key to determine the implants that it is exclusive to.
    /// </summary>
    [DataField(required: true)]
    public string ExclusiveKey;
}
