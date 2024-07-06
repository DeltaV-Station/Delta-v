using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Server.DeltaV.CartridgeLoader.Cartridges;

public sealed class MailMetricsCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly LogisticStatsSystem _logisticsStats = default!;
    [Dependency] private readonly StationSystem _station = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("mail");

        SubscribeLocalEvent<MailMetricsCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<LogisticStatsUpdatedEvent>(OnLogisticsStatsUpdated);
    }

    private void OnUiReady(Entity<MailMetricsCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        _sawmill.Debug("mail UI ready");
        UpdateUI(ent, args.Loader);
    }

    private void OnLogisticsStatsUpdated(LogisticStatsUpdatedEvent args)
    {
        _sawmill.Debug("updated logi stats");
        var query = EntityQueryEnumerator<MailMetricsCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is not { } loader || comp.Station != args.Station)
                continue;
            UpdateUI((uid, comp), loader);
        }
    }

    private void UpdateUI(Entity<MailMetricsCartridgeComponent> ent, EntityUid loader)
    {
        _sawmill.Debug("updating mail UI");
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        if (!TryComp<StationLogisticStatsComponent>(ent.Comp.Station, out var logiStats))
            return;

        // Get station's logistic stats
        var mailEarnings = logiStats.MailEarnings;

        // Send logistic stats to cartridge client
        var state = new MailMetricUiState(mailEarnings);
        _cartridgeLoader.UpdateCartridgeUiState(loader, state);
    }
}
