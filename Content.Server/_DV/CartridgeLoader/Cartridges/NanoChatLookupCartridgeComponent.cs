namespace Content.Server._DV.CartridgeLoader.Cartridges;

[RegisterComponent, Access(typeof(NanoChatLookupCartridgeSystem))]
public sealed partial class NanoChatLookupCartridgeComponent : Component
{
    /// <summary>
    ///     Station entity to keep track of.
    /// </summary>
    [DataField]
    public EntityUid? Station;
}
