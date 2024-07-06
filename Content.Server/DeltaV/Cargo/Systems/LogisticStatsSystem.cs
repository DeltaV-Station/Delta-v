using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.CartridgeLoader.Cartridges;
using Content.Server.Station.Systems;
using Content.Shared.Cargo;
using JetBrains.Annotations;
using Robust.Server.GameObjects;

namespace Content.Server.DeltaV.Cargo.Systems;

public sealed partial class LogisticStatsSystem : SharedCargoSystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [PublicAPI]
    public void AddMailEarnings(EntityUid uid, StationLogisticStatsComponent component, int earnedMoney)
    {
        component.MailEarnings += earnedMoney;
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
