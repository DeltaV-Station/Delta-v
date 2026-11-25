using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Server._DV.PowerCell;

public sealed class PowerCellSearchSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCellSlotComponent, SearchForBatteryEvent>(OnSearchForBattery);
    }

    private void OnSearchForBattery(Entity<PowerCellSlotComponent> ent, ref SearchForBatteryEvent args)
    {
        if (_powerCell.TryGetBatteryFromSlot(ent.AsNullable(), out var battery))
        {
            args.Uid = battery.Value.Owner;
            args.Component = battery.Value.Comp;
            args.Handled = true;
        }
        else
            args.Handled = false;
    }
}
