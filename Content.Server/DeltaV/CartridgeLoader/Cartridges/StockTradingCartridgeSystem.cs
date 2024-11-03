using Content.Server.Cargo.Components;
using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Server.CartridgeLoader;
using Content.Shared.Cargo.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Server.DeltaV.CartridgeLoader.Cartridges;

public sealed class StockTradingCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StockTradingCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<StockMarketUpdatedEvent>(OnStockMarketUpdated);
        SubscribeLocalEvent<StationStockMarketComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StockTradingCartridgeComponent, BankBalanceUpdatedEvent>(OnBalanceUpdated);
    }

    private void OnBalanceUpdated(Entity<StockTradingCartridgeComponent> ent, ref BankBalanceUpdatedEvent args)
    {
            UpdateAllCartridges(args.Station);
    }

    private void OnUiReady(Entity<StockTradingCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void OnStockMarketUpdated(StockMarketUpdatedEvent args)
    {
        UpdateAllCartridges(args.Station);
    }

    private void OnMapInit(EntityUid uid, StationStockMarketComponent stockMarket, MapInitEvent args)
    {
        if (_station.GetOwningStation(uid) is { } station)
            UpdateAllCartridges(station);
    }

    private void UpdateAllCartridges(EntityUid station)
    {
        var query = EntityQueryEnumerator<StockTradingCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is not { } loader || comp.Station != station)
                continue;
            UpdateUI((uid, comp), loader);
        }
    }

    private void UpdateUI(Entity<StockTradingCartridgeComponent> ent, EntityUid loader)
    {
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        if (!TryComp<StationStockMarketComponent>(ent.Comp.Station, out var stockMarket) ||
            !TryComp<StationBankAccountComponent>(ent.Comp.Station, out var bankAccount))
            return;

        // Convert company data to UI state format
        var entries = new List<StockCompanyStruct>();

        foreach (var (key, company) in stockMarket.Companies)
        {
            entries.Add(new StockCompanyStruct(
                name: company.Name,
                currentPrice: company.CurrentPrice,
                basePrice: company.BasePrice,
                priceHistory: company.PriceHistory
            ));
        }

        // Send the UI state with balance and owned stocks
        var state = new StockTradingUiState(
            entries: entries,
            ownedStocks: stockMarket.StockOwnership,
            balance: bankAccount.Balance
        );

        _cartridgeLoader.UpdateCartridgeUiState(loader, state);
    }
}
