using Robust.Shared.GameStates;

namespace Content.Shared._DV.Implants.Radio;

/// <summary>
/// This indicates this entity has a radio implant implanted into themselves.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedRadioImplantSystem))]
[AutoGenerateComponentState]
public sealed partial class HasRadioImplantComponent : Component
{
    /// <summary>
    /// A list of radio implants. We need this to be able to determine encryption keys.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Implants = new();
}
