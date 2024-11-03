using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Database;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Cargo.Systems;

/// <summary>
/// This handles the stock market updates
/// </summary>
public sealed class StockMarketSystem : EntitySystem
{
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private ISawmill _sawmill = default!;
    private const float MaxPrice = 262144; // 1/64 of max safe integer

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _log.GetSawmill("admin.stock_market");

        SubscribeLocalEvent<StockTradingCartridgeComponent, CartridgeMessageEvent>(OnStockTradingMessage);
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<StationStockMarketComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdate)
                continue;

            component.NextUpdate = curTime + component.UpdateInterval;
            UpdateStockPrices(uid, component);
        }
    }

    private void OnStockTradingMessage(Entity<StockTradingCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not StockTradingUiMessageEvent message)
            return;

        var name = message.Company;
        var amount = (int)message.Amount; // Convert to int since we can't have partial shares
        var station = ent.Comp.Station;

        if (station == null)
            return;

        // Check if the station has a stock market
        if (!TryComp<StationStockMarketComponent>(station, out var stockMarket))
            return;

        // Validate company exists
        if (!stockMarket.Companies.ContainsKey(name))
            return;

        // Log who's doing the transaction
        // This will only display the PDA from which it was done, and not the user, but perhaps still somewhat useful for admins??
        var loader = args.LoaderUid;

        switch (message.Action)
        {
            case StockTradingUiAction.Buy:
                _adminLogger.Add(LogType.Action,
                    LogImpact.Medium,
                    $"{ToPrettyString(loader)} attempting to buy {amount} stocks of {name}");
                BuyStocks(station.Value, stockMarket, name, amount);
                break;
            case StockTradingUiAction.Sell:
                _adminLogger.Add(LogType.Action,
                    LogImpact.Medium,
                    $"{ToPrettyString(loader)} attempting to sell {amount} stocks of {name}");
                SellStocks(station.Value, stockMarket, name, amount);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Update UI
        var ev = new StockMarketUpdatedEvent(station.Value);
        RaiseLocalEvent(ev);
    }

    private void BuyStocks(
        EntityUid station,
        StationStockMarketComponent stockMarket,
        string companyName,
        int amount)
    {
        if (amount <= 0)
            return;

        // Check if the station has a bank account
        if (!TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        // Check if the company exists
        if (!stockMarket.Companies.TryGetValue(companyName, out var company))
            return;

        // Convert to int
        var totalValue = (int)Math.Round(company.CurrentPrice * amount);

        // Update stock ownership
        if (!stockMarket.StockOwnership.TryGetValue(companyName, out var currentOwned))
            currentOwned = 0;

        // Update the bank account
        _cargo.UpdateBankAccount(station, bank, -totalValue);

        stockMarket.StockOwnership[companyName] = currentOwned + amount;

        // Log the transaction
        _adminLogger.Add(LogType.Action,
            LogImpact.Medium,
            $"[StockMarket] Bought {amount} stocks of {companyName} at {company.CurrentPrice:F2} credits each (Total: {totalValue})");
    }

    private void SellStocks(
        EntityUid station,
        StationStockMarketComponent stockMarket,
        string companyName,
        int amount)
    {
        if (amount <= 0)
            return;

        // Check if the station has a bank account
        if (!TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        // Check if the company exists
        if (!stockMarket.Companies.TryGetValue(companyName, out var company))
            return;

        // Check if the station owns enough stocks
        if (!stockMarket.StockOwnership.TryGetValue(companyName, out var currentOwned) || currentOwned < amount)
            return;

        // Convert to int
        var totalValue = (int)Math.Round(company.CurrentPrice * amount);

        // Update stock ownership
        var newAmount = currentOwned - amount;
        if (newAmount > 0)
            stockMarket.StockOwnership[companyName] = newAmount;
        else
            stockMarket.StockOwnership.Remove(companyName);

        // Update the bank account
        _cargo.UpdateBankAccount(station, bank, totalValue);

        // Log the transaction
        _adminLogger.Add(LogType.Action,
            LogImpact.Medium,
            $"[StockMarket] Sold {amount} stocks of {companyName} at {company.CurrentPrice:F2} credits each (Total: {totalValue})");
    }

    private void UpdateStockPrices(EntityUid station, StationStockMarketComponent stockMarket)
    {
        var companies = stockMarket.Companies;

        foreach (var key in companies.Keys.ToList())
        {
            var company = companies[key];
            var changeType = DetermineChangeType(stockMarket);
            var multiplier = CalculatePriceMultiplier(changeType, stockMarket);

            UpdatePriceHistory(company);

            // Update price with multiplier
            var oldPrice = company.CurrentPrice;
            company.CurrentPrice *= (1 + multiplier);

            // Ensure price doesn't go below minimum threshold
            company.CurrentPrice = MathF.Max(company.CurrentPrice, company.BasePrice * 0.1f);

            // Ensure price doesn't go above maximum threshold
            company.CurrentPrice = MathF.Min(company.CurrentPrice, MaxPrice);

            // Save the modified struct back to the dictionary
            companies[key] = company;

            // Calculate the percentage change
            var percentChange = (company.CurrentPrice - oldPrice) / oldPrice * 100;

            // Raise the event
            var ev = new StockMarketUpdatedEvent(station);
            RaiseLocalEvent(ev);

            // Log it
            _adminLogger.Add(LogType.Action,
                LogImpact.Medium,
                $"[StockMarket] Company '{company.Name}' price updated by {percentChange:+0.00;-0.00}% from {oldPrice:0.00} to {company.CurrentPrice:0.00}");
        }
    }

    /// <summary>
    /// Attempts to change the price for a specific company
    /// </summary>
    /// <returns>True if the operation was successful, false otherwise</returns>
    public bool TryChangeStocksPrice(EntityUid station,
        StationStockMarketComponent stockMarket,
        float newPrice,
        string companyName)
    {
        // Check if it exceeds the max price
        if (newPrice > MaxPrice)
        {
            _sawmill.Error($"New price cannot be greater than {MaxPrice}.");
            return false;
        }

        var companies = stockMarket.Companies;
        foreach (var key in companies.Keys.ToList())
        {
            var company = companies[key];

            // Continue if it doesn't match the company we're looking for
            if (company.Name != companyName)
                continue;

            UpdatePriceHistory(company);

            // Update price
            company.CurrentPrice = newPrice;

            // Ensure it doesn't go below minimum threshold
            company.CurrentPrice = MathF.Max(company.CurrentPrice, company.BasePrice * 0.1f);

            // Save the modified struct back to the dictionary
            companies[key] = company;

            var ev = new StockMarketUpdatedEvent(station);
            RaiseLocalEvent(ev);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to add a new company to the station
    /// </summary>
    /// <returns>False if the company already exists, true otherwise</returns>
    public bool TryAddCompany(EntityUid station,
        StationStockMarketComponent stockMarket,
        float basePrice,
        string companyName)
    {
    var companies = stockMarket.Companies;

    // Check if the company already exists in the dictionary
    if (companies.ContainsKey(companyName))
    {
        return false;
    }

    // Create a new company struct with the specified parameters
    var company = new StockCompanyStruct
    {
        Name = companyName,
        BasePrice = basePrice,
        CurrentPrice = basePrice,
        PriceHistory = [],
    };

    // Add the new company to the dictionary
    companies[companyName] = company;

    var ev = new StockMarketUpdatedEvent(station);
    RaiseLocalEvent(ev);

    return true;
    }

    private static void UpdatePriceHistory(StockCompanyStruct company)
    {
        // Store previous price in history
        company.PriceHistory.Add(company.CurrentPrice);

        if (company.PriceHistory.Count > 5) // Keep last 5 prices
            company.PriceHistory.RemoveAt(0);
    }

    private StockChangeType DetermineChangeType(StationStockMarketComponent stockMarket)
    {
        var roll = _random.NextFloat();

        if (roll < stockMarket.CatastrophicChangeChance)
            return StockChangeType.Catastrophic;

        roll -= stockMarket.CatastrophicChangeChance;

        if (roll < stockMarket.MajorChangeChance)
            return StockChangeType.Major;

        roll -= stockMarket.MajorChangeChance;

        if (roll < stockMarket.ModerateChangeChance)
            return StockChangeType.Moderate;

        return StockChangeType.Minor;
    }

    private float CalculatePriceMultiplier(StockChangeType changeType, StationStockMarketComponent stockMarket)
    {
        var (min, max) = changeType switch
        {
            StockChangeType.Minor => stockMarket.MinorChangeRange,
            StockChangeType.Moderate => stockMarket.ModerateChangeRange,
            StockChangeType.Major => stockMarket.MajorChangeRange,
            StockChangeType.Catastrophic => stockMarket.CatastrophicChangeRange,
            _ => throw new ArgumentOutOfRangeException(nameof(changeType)),
        };

        // Using Box-Muller transform for normal distribution
        var u1 = _random.NextFloat();
        var u2 = _random.NextFloat();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        // Scale and shift the result to our desired range
        var range = max - min;
        var mean = (max + min) / 2;
        var stdDev = range / 6.0f; // 99.7% of values within range

        var result = (float)(mean + (stdDev * randStdNormal));
        return Math.Clamp(result, min, max);
    }

    private enum StockChangeType
    {
        Minor,
        Moderate,
        Major,
        Catastrophic,
    }
}
public sealed class StockMarketUpdatedEvent(EntityUid station) : EntityEventArgs
{
    public EntityUid Station = station;
}
