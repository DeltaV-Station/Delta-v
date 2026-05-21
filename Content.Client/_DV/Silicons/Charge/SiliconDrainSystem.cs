using Content.Client.Alerts;
using Content.Shared._EE.Silicon.Components;
using Content.Shared._DV.Silicons.Charge;

namespace Content.Client._DV.Silicon.Charge;

public sealed class SiliconDrainSystem : SharedSiliconDrainSystem
{
    [Dependency] private readonly ClientAlertsSystem _alerts = default!;

    protected override void UpdateChargeIcon(Entity<SiliconComponent> ent, short chargePercent)
    {
        // If you can't find a battery, set the indicator and skip it.
        if (!TryGetSiliconBattery(ent, out _))
        {
            if (_alerts.IsShowingAlert(ent.Owner, ent.Comp.BatteryAlert))
            {
                _alerts.ClearAlert(ent.Owner, ent.Comp.BatteryAlert);
                _alerts.ShowAlert(ent.Owner, ent.Comp.NoBatteryAlert);
            }
        }
        // If the battery was replaced and the no battery indicator is showing, replace the indicator
        else if (_alerts.IsShowingAlert(ent.Owner, ent.Comp.NoBatteryAlert))
        {
            _alerts.ClearAlert(ent.Owner, ent.Comp.NoBatteryAlert);
            _alerts.ShowAlert(ent.Owner, ent.Comp.BatteryAlert, chargePercent);
        }
    }
}
