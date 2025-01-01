using Robust.Shared.GameStates;

namespace Content.Shared._DV.Implants.Radio;

/// <summary>
/// This indicates this entity has a radio implant implanted into themselves.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedRadioImplantSystem))]
public sealed partial class HasRadioImplantComponent : Component
{
    /// <summary>
    /// The radio implant. We need this to be able to determine encryption keys.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Implant;
}
