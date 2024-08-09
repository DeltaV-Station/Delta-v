using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.CartridgeLoader.Cartridges;
using Content.Server.Station.Systems;
using Content.Shared.Cargo;
using JetBrains.Annotations;
using Robust.Server.GameObjects;

namespace Content.Server.DeltaV.Cargo.Systems;

public sealed partial class LogisticStatsSystem : SharedCargoSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [PublicAPI]
    public void AddOpenedMailEarnings(EntityUid uid, StationLogisticStatsComponent component, int earnedMoney)
    {
        component.MailEarnings += earnedMoney;
        component.OpenedMailCount += 1;
        UpdateLogisticsStats(uid);
    }

    [PublicAPI]
    public void AddExpiredMailLosses(EntityUid uid, StationLogisticStatsComponent component, int lostMoney)
    {
        component.ExpiredMailLosses += lostMoney;
        component.ExpiredMailCount += 1;
        UpdateLogisticsStats(uid);
    }

    [PublicAPI]
    public void AddDamagedMailLosses(EntityUid uid, StationLogisticStatsComponent component, int lostMoney)
    {
        component.DamagedMailLosses += lostMoney;
        component.DamagedMailCount += 1;
        UpdateLogisticsStats(uid);
    }

    [PublicAPI]
    public void AddTamperedMailLosses(EntityUid uid, StationLogisticStatsComponent component, int lostMoney)
    {
        component.TamperedMailLosses += lostMoney;
        component.TamperedMailCount += 1;
        UpdateLogisticsStats(uid);
    }

    private void UpdateLogisticsStats(EntityUid uid)
    {
        RaiseLocalEvent(new LogisticStatsUpdatedEvent(uid));
    }
}

public sealed class LogisticStatsUpdatedEvent : EntityEventArgs
{
    public EntityUid Station;
    public LogisticStatsUpdatedEvent(EntityUid station)
    {
        Station = station;
    }
}
