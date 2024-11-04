using System.Numerics;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Shared.Audio;
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
            DisplayName = "Nanotrasen [NT]",
            CurrentPrice = 100f,
            BasePrice = 100f,
            PriceHistory = [100f, 100f, 100f, 100f, 100f], // look somewhere else
        },
        ["Gorlex"] = new StockCompanyStruct
        {
            Name = "Gorlex",
            DisplayName = "Gorlex [GRX]",
            CurrentPrice = 75f,
            BasePrice = 75f,
            PriceHistory = [75f, 75f, 75f, 75f, 75f],
        },
        ["FishInc"] = new StockCompanyStruct
        {
            Name = "FishInc",
            DisplayName = "Fish Inc. [FIN]",
            CurrentPrice = 25f,
            BasePrice = 25f,
            PriceHistory = [25f, 25f, 25f, 25f, 25f],
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
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(10); // 5 minutes

    /// <summary>
    /// The <see cref="IGameTiming.CurTime"/> timespan of next update.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// The sound to play after selling or buying stocks
    /// </summary>
    [DataField]
    public SoundSpecifier ConfirmSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound to play if the don't have access to buy or sell stocks
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    /// <summary>
    /// The chance for minor market changes
    /// </summary>
    [DataField]
    public float MinorChangeChance = 0.86f; // 86%

    /// <summary>
    /// The chance for moderate market changes
    /// </summary>
    [DataField]
    public float ModerateChangeChance = 0.10f; // 10%

    /// <summary>
    /// The chance for major market changes
    /// </summary>
    [DataField]
    public float MajorChangeChance = 0.03f; // 3%

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
    public Vector2 ModerateChangeRange = new(-0.3f, 0.2f); // -30% to +20%

    /// <summary>
    /// The price range for major changes
    /// </summary>
    [DataField]
    public Vector2 MajorChangeRange = new(-0.5f, 1.5f); // -50% to +150%

    /// <summary>
    /// The price range for catastrophic changes
    /// </summary>
    [DataField]
    public Vector2 CatastrophicChangeRange = new(-0.9f, 4.0f); // -90% to +400%
}
