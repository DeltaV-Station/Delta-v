using Content.Server.Station.Systems;
using Content.Server.CartridgeLoader;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared._DV.Cargo.Components;
using Content.Shared._DV.Cargo.Systems;
using Content.Shared._DV.Mail;
using Content.Shared.CartridgeLoader;
using Content.Shared.Station.Components;

namespace Content.Server._DV.CartridgeLoader.Cartridges;

public sealed class MailMetricsCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailMetricsCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<LogisticStatsUpdatedEvent>(OnLogisticsStatsUpdated);
        SubscribeLocalEvent<MailComponent, MapInitEvent>(OnMapInit);
    }

    private void OnUiReady(Entity<MailMetricsCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void OnLogisticsStatsUpdated(ref LogisticStatsUpdatedEvent args)
    {
        UpdateAllCartridges(args.Station);
    }

    private void OnMapInit(EntityUid uid, MailComponent mail, MapInitEvent args)
    {
        if (_station.GetOwningStation(uid) is { } station)
            UpdateAllCartridges(station);
    }

    private void UpdateAllCartridges(EntityUid station)
    {
        var query = EntityQueryEnumerator<MailMetricsCartridgeComponent, CartridgeComponent, StationTrackerComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge, out var stationTracker))
        {
            if (cartridge.LoaderUid is not { } loader || stationTracker.Station != station)
                continue;
            UpdateUI((uid, comp, stationTracker), loader);
        }
    }

    private void UpdateUI(Entity<MailMetricsCartridgeComponent, StationTrackerComponent?> ent, EntityUid loader)
    {
        if (!Resolve(ent, ref ent.Comp2))
            return;

        if (!TryComp<StationLogisticStatsComponent>(ent.Comp2.Station, out var logiStats))
            return;

        // Get station's logistic stats
        var unopenedMailCount = GetUnopenedMailCount(ent.Comp2.Station);

        // Send logistic stats to cartridge client
        var state = new MailMetricUiState(logiStats.Metrics, unopenedMailCount);
        _cartridgeLoader.UpdateCartridgeUiState(loader, state);
    }


    private int GetUnopenedMailCount(EntityUid? station)
    {
        var unopenedMail = 0;

        var query = EntityQueryEnumerator<MailComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsLocked && _station.GetOwningStation(uid) == station)
                unopenedMail++;
        }

        return unopenedMail;
    }
}
