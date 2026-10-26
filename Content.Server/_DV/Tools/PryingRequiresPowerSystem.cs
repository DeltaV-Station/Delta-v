using Content.Server.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Prying.Components;
using Robust.Shared.Utility;

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

    private void OnPried(Entity<PryingRequiresPowerComponent> ent, ref PriedEvent args)
    {
        // Entity has a PowerCellSlot, try that first
        if (!_cell.TryGetBatteryFromSlotOrEntity(ent.Owner, out var battery))
        {
            DebugTools.Assert($"{ent} has pried something open without a battery or cell.");
            return;
        }

        // The entity itself is a battery
        _battery.TryUseCharge(battery.Value.AsNullable(), ent.Comp.PowerCost);
    }

    private void OnBeforePry(Entity<PryingRequiresPowerComponent> ent, ref BeforePryEvent args)
    {
        if (!_cell.TryGetBatteryFromSlotOrEntity(ent.Owner, out var battery))
        {
            args.Cancelled = true;
            return;
        }

        if (_battery.GetCharge(battery.Value.AsNullable()) < ent.Comp.PowerCost)
            args.Cancelled = true;
    }
}
