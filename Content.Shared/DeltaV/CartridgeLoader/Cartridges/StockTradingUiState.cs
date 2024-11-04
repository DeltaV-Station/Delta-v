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
    /// <summary>
    /// The internal name/key of the company. Should not contain spaces or special characters.
    /// </summary>
    [DataField(required: true)]
    public string Name;

    /// <summary>
    /// The displayed name of the company shown in the UI. Can contain spaces and special characters.
    /// </summary>
    [DataField(required: true)]
    public string DisplayName;

    /// <summary>
    /// The current price of the company's stock
    /// </summary>
    [DataField]
    public float CurrentPrice;

    /// <summary>
    /// The base price of the company's stock
    /// </summary>
    [DataField]
    public float BasePrice;

    /// <summary>
    /// The price history of the company's stock
    /// </summary>
    [DataField]
    public List<float> PriceHistory;

    public StockCompanyStruct(string name, string displayName, float currentPrice, float basePrice, List<float> priceHistory)
    {
        Name = name;
        DisplayName = displayName;
        CurrentPrice = currentPrice;
        BasePrice = basePrice;
        PriceHistory = priceHistory;
    }
}
