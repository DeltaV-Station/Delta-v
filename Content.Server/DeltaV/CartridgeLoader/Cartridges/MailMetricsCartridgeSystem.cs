using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Server.Mail.Components;

namespace Content.Server.DeltaV.CartridgeLoader.Cartridges;

public sealed class MailMetricsCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly LogisticStatsSystem _logisticsStats = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailMetricsCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<LogisticStatsUpdatedEvent>(OnLogisticsStatsUpdated);
    }

    private void OnUiReady(Entity<MailMetricsCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void OnLogisticsStatsUpdated(LogisticStatsUpdatedEvent args)
    {
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
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        if (!TryComp<StationLogisticStatsComponent>(ent.Comp.Station, out var logiStats))
            return;

        // Get station's logistic stats
        var mailEarnings = logiStats.MailEarnings;
        var damagedMailLosses = logiStats.DamagedMailLosses;
        var expiredMailLosses = logiStats.ExpiredMailLosses;
        var tamperedMailLosses = logiStats.TamperedMailLosses;
        var openedMailCount = logiStats.OpenedMailCount;
        var damagedMailCount = logiStats.DamagedMailCount;
        var expiredMailCount = logiStats.ExpiredMailCount;
        var tamperedMailCount = logiStats.TamperedMailCount;
        var unopenedMailCount = GetUnopenedMailCount();

        // Send logistic stats to cartridge client
        var state = new MailMetricUiState(mailEarnings,
                                          damagedMailLosses,
                                          expiredMailLosses,
                                          tamperedMailLosses,
                                          openedMailCount,
                                          damagedMailCount,
                                          expiredMailCount,
                                          tamperedMailCount,
                                          unopenedMailCount);
        _cartridgeLoader.UpdateCartridgeUiState(loader, state);
    }

    private int GetUnopenedMailCount()
    {
        var unopenedMail = 0;
        var query = EntityQueryEnumerator<MailComponent>();
        while (query.MoveNext(out var station, out var mail))
        {
            if (!mail.IsEnabled)
                unopenedMail += 1;
        }

        return unopenedMail;
    }
}
