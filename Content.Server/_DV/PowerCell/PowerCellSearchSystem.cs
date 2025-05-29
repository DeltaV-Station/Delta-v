using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
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
        args.Handled = _powerCell.TryGetBatteryFromSlot(ent, out args.Uid, out args.Component);
    }
}
