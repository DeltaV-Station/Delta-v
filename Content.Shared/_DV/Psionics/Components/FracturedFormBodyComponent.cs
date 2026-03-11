using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent]
public sealed partial class FracturedFormBodyComponent : Component
{
    /// <summary>
    /// The entity uid of the form that is currently being controlled.
    /// </summary>
    [DataField]
    public EntityUid ControllingForm { get; set; }
}
