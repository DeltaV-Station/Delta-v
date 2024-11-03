using System.Numerics;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Cargo.Components;

[RegisterComponent]
public sealed partial class StationStockMarketComponent : Component
{
    /// <summary>
    /// The list of companies you can invest in
    /// </summary>
    [DataField]
    public Dictionary<string, StockCompanyStruct> Companies = new()
    {
        ["Nanotrasen"] = new StockCompanyStruct
        {
            Name = "Nanotrasen",
            CurrentPrice = 100f,
            BasePrice = 100f,
            PriceHistory = [],
        },
        ["Gorlex"] = new StockCompanyStruct
        {
            Name = "Gorlex",
            CurrentPrice = 75f,
            BasePrice = 75f,
            PriceHistory = [],
        },
        ["FishInc"] = new StockCompanyStruct
        {
            Name = "Fish Inc.",
            CurrentPrice = 25f,
            BasePrice = 25f,
            PriceHistory = [],
        },
    };

    /// <summary>
    /// The list of shares owned by the station
    /// </summary>
    [DataField]
    public Dictionary<string, int> StockOwnership = new();

    /// <summary>
    /// The interval at which the stock market updates
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(60); // 5 minutes

    /// <summary>
    /// The <see cref="IGameTiming.CurTime"/> timespan of next update.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// The chance for minor market changes
    /// </summary>
    [DataField]
    public float MinorChangeChance = 0.70f; // 70%

    /// <summary>
    /// The chance for moderate market changes
    /// </summary>
    [DataField]
    public float ModerateChangeChance = 0.25f; // 25%

    /// <summary>
    /// The chance for major market changes
    /// </summary>
    [DataField]
    public float MajorChangeChance = 0.04f; // 4%

    /// <summary>
    /// The chance for catastrophic market changes
    /// </summary>
    [DataField]
    public float CatastrophicChangeChance = 0.01f; // 1%

    /// <summary>
    /// The price range for minor changes
    /// </summary>
    [DataField]
    public Vector2 MinorChangeRange = new(-0.05f, 0.05f); // -5% to +5%

    /// <summary>
    /// The price range for moderate changes
    /// </summary>
    [DataField]
    public Vector2 ModerateChangeRange = new(-0.2f, 0.4f); // -20% to +40%

    /// <summary>
    /// The price range for major changes
    /// </summary>
    [DataField]
    public Vector2 MajorChangeRange = new(-0.5f, 2.0f); // -50% to +200%

    /// <summary>
    /// The price range for catastrophic changes
    /// </summary>
    [DataField]
    public Vector2 CatastrophicChangeRange = new(-0.9f, 4.0f); // -90% to +400%
}
