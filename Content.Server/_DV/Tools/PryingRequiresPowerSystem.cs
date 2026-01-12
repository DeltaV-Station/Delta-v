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

    private void OnPried(Entity<PryingRequiresPowerComponent> ent, ref PriedEvent args)
    {
        // Entity has a PowerCellSlot, try that first
        if (_cell.TryUseCharge(ent.Owner, ent.Comp.PowerCost))
            return;

        // The entity itself is a battery
        _battery.TryUseCharge(ent.Owner, ent.Comp.PowerCost);
    }

    private void OnBeforePry(Entity<PryingRequiresPowerComponent> ent, ref BeforePryEvent args)
    {
        if (!_cell.HasCharge(ent, ent.Comp.PowerCost, null, args.User))
            args.Cancelled = true;
    }
}
