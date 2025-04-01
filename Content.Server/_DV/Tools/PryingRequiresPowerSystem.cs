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
        // Tool is a battery use its power
        if (_battery.TryUseCharge(ent, ent.Comp.PowerCost))
            return;

        // Tool has a battery in a power cell slot use that power
        if (_cell.TryGetBatteryFromSlot(ent, out var batteryEnt, out var batteryComp)
            && batteryEnt is { } batteryUid)
        {
            _battery.TryUseCharge(batteryUid, ent.Comp.PowerCost, batteryComp);
            return;
        }
    }

    private void OnBeforePry(Entity<PryingRequiresPowerComponent> ent, ref BeforePryEvent args)
    {
        if (!_cell.HasCharge(ent, ent.Comp.PowerCost, null, args.User))
            args.Cancelled = true;
    }
}
