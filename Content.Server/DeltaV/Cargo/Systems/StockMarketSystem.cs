using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.DeltaV.Cargo.Components;
using Content.Shared.Database;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Cargo.Systems;

/// <summary>
/// This handles the stock market updates
/// </summary>
public sealed class StockMarketSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;


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

    private void UpdateStockPrices(EntityUid station, StationStockMarketComponent stockMarket)
    {
        var companies = stockMarket.Companies;

        foreach (var key in companies.Keys.ToList())
        {
            var company = companies[key];
            var changeType = DetermineChangeType(stockMarket);
            var multiplier = CalculatePriceMultiplier(changeType, stockMarket);

            // Store previous price in history
            company.PriceHistory.Add(company.CurrentPrice);

            if (company.PriceHistory.Count > 10) // Keep last 10 prices
                company.PriceHistory.RemoveAt(0);

            // Update price with multiplier
            var oldPrice = company.CurrentPrice;
            company.CurrentPrice *= (1 + multiplier);

            // Ensure price doesn't go below minimum threshold
            company.CurrentPrice = MathF.Max(company.CurrentPrice, company.BasePrice * 0.1f);

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
