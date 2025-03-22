using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Implants.Radio;

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
    public EntityUid? Implantee;

    /// <summary>
    /// The channels this implant can talk on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<RadioChannelPrototype>> Channels = new();
}
