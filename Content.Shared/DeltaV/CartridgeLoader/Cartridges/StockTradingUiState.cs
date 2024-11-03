using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class StockTradingUiState(
    List<StockCompanyStruct> entries,
    Dictionary<string, int> ownedStocks,
    float balance)
    : BoundUserInterfaceState
{
    public readonly List<StockCompanyStruct> Entries = entries;
    public readonly Dictionary<string, int> OwnedStocks = ownedStocks;
    public readonly float Balance = balance;
}

// No structure, zero fucks given
[DataDefinition, Serializable]
public partial struct StockCompanyStruct
{
    [DataField(required: true)]
    public string Name;

    [DataField]
    public float CurrentPrice;

    [DataField]
    public float BasePrice;

    [DataField]
    public List<float> PriceHistory;

    public StockCompanyStruct( string name, float currentPrice, float basePrice, List<float> priceHistory)
    {
        Name = name;
        CurrentPrice = currentPrice;
        BasePrice = basePrice;
        PriceHistory = priceHistory;
    }
}
