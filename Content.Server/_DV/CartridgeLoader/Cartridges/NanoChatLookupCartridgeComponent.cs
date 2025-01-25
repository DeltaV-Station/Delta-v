using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CartridgeLoader.Cartridges;

[RegisterComponent, Access(typeof(NanoChatLookupCartridgeSystem))]
public sealed partial class NanoChatLookupCartridgeComponent : Component
{
    /// <summary>
    ///     The <see cref="RadioChannelPrototype" /> to scan for contacts.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Common";
}
