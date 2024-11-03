namespace Content.Server.DeltaV.CartridgeLoader.Cartridges;

[RegisterComponent, Access(typeof(StockTradingCartridgeSystem))]
public sealed partial class StockTradingCartridgeComponent : Component
{
    /// <summary>
    /// Station entity keeping track of
    /// </summary>
    [DataField]
    public EntityUid? Station;
}
