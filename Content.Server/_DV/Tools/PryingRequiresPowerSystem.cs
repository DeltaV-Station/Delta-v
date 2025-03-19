using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared.Prying.Components;

namespace Content.Server._DV.Tools;

public sealed class PryingRequiresPowerSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PryingRequiresPowerComponent, BeforePryEvent>(OnBeforePry);
        SubscribeLocalEvent<PryingRequiresPowerComponent, PriedEvent>(OnPried);
    }

    private void OnPried(EntityUid uid, PryingRequiresPowerComponent component, PriedEvent args)
    {
        BatteryComponent? batteryComp;
        // Tool is a battery use its power
        if (TryComp<BatteryComponent>(uid, out batteryComp))
        {
            _battery.UseCharge(uid, component.PowerCost, batteryComp);
            return;
        }

        // Tool has a battery in a power cell slot use that power
        if (_cell.TryGetBatteryFromSlot(uid, out var batteryEnt, out batteryComp))
        {
            _battery.UseCharge((EntityUid)batteryEnt, component.PowerCost, batteryComp);
            return;
        }
    }

    private void OnBeforePry(EntityUid uid, PryingRequiresPowerComponent component, ref BeforePryEvent args)
    {
        if (!_cell.HasCharge(uid, component.PowerCost, null, args.User))
            args.Cancelled = true;
    }
}
