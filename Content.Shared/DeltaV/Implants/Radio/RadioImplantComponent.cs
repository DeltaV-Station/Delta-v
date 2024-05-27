using Robust.Shared.GameStates;

namespace Content.Shared.DeltaV.Implants.Radio;

/// <summary>
/// This is for radio implants. Might be Syndie, might not be Syndie, but either way, it's an implant.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedRadioImplantSystem))]
public sealed partial class RadioImplantComponent : Component
{
    /// <summary>
    /// The entity this implant got added to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Implantee { get; set; }

    /// <summary>
    /// The channels this implant can talk on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<string> Channels { get; set; } = new();
}
