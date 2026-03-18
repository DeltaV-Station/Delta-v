using Content.Client.Alerts;
using Content.Shared._EE.Silicon.Components;
using Content.Shared._EE.Silicon.Systems;

namespace Content.Client._DV.Silicon.Charge;

public sealed class SiliconChargeSystem : SharedSiliconChargeSystem
{
    [Dependency] private readonly ClientAlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, ComponentInit>(OnSiliconInit);
        SubscribeLocalEvent<SiliconComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
    }

    private void OnSiliconInit(EntityUid uid, SiliconComponent component, ComponentInit args)
    {
        if (!component.BatteryPowered)
            return;

        _alerts.ShowAlert(uid, component.BatteryAlert, component.ChargeState);
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconComponent component, SiliconChargeStateUpdateEvent ev)
    {
        _alerts.ShowAlert(uid, component.BatteryAlert, ev.ChargePercent);
    }
}
