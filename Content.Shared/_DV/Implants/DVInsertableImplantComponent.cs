using Robust.Shared.GameStates;

namespace Content.Shared._DV.Implants;

/// <summary>
/// Entities with this component can be inserted into implanters.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(DVInsertableImplantSystem))]
public sealed partial class DVInsertableImplantComponent : Component
{
    /// <summary>
    /// The name to change the implanter to upon insertion.
    /// </summary>
    [DataField(required: true)]
    public LocId ImplanterName;

    /// <summary>
    /// The description to change the implanter to upon insertion.
    /// </summary>
    [DataField(required: true)]
    public LocId ImplanterDescription;
}
