using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Tools.Components;

namespace Content.Shared.Tools.Systems;

public abstract partial class SharedToolSystem
{
    public void InitializeSolutionRefuel()
    {
        SubscribeLocalEvent<SolutionRefuelComponent, ExaminedEvent>(SolutionRefuelExamine);
        SubscribeLocalEvent<SolutionRefuelComponent, AfterInteractEvent>(OnSolutionRefuelAfterInteract);
    }

    public (FixedPoint2 fuel, FixedPoint2 capacity) GetSolutionFuelAndCapacity(EntityUid uid, SolutionRefuelComponent? welder = null, SolutionContainerManagerComponent? solutionContainer = null)
    {
        if (!Resolve(uid, ref welder, ref solutionContainer))
            return default;

        if (!SolutionContainerSystem.TryGetSolution(
                (uid, solutionContainer),
                welder.FuelSolutionName,
                out _,
                out var fuelSolution))
        {
            return default;
        }

        return (fuelSolution.GetTotalPrototypeQuantity(welder.FuelReagent), fuelSolution.MaxVolume);
    }

    private void SolutionRefuelExamine(Entity<SolutionRefuelComponent> entity, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(SolutionRefuelComponent)))
        {
            if (args.IsInDetailsRange)
            {
                var (fuel, capacity) = GetSolutionFuelAndCapacity(entity.Owner, entity.Comp);

                args.PushMarkup(Loc.GetString("solution-refuel-component-on-examine-detailed-message",
                    ("colorName", fuel < capacity / FixedPoint2.New(4f) ? "darkorange" : "orange"),
                    ("fuelLeft", fuel),
                    ("fuelCapacity", capacity)));
            }
        }
    }

    private void OnSolutionRefuelAfterInteract(Entity<SolutionRefuelComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { Valid: true } target || !args.CanReach)
            return;

        if (TryComp(target, out ReagentTankComponent? tank)
            && tank.TankType == ReagentTankType.Fuel
            && SolutionContainerSystem.TryGetDrainableSolution(target, out var targetSoln, out var targetSolution)
            && _whitelist.CheckBoth(entity, tank.FuelBlacklist, tank.FuelWhitelist)
            && SolutionContainerSystem.TryGetSolution(entity.Owner, entity.Comp.FuelSolutionName, out var solutionComp, out var welderSolution))
        {
            var trans = FixedPoint2.Min(welderSolution.AvailableVolume, targetSolution.Volume);
            if (trans > 0)
            {
                var drained = SolutionContainerSystem.Drain(target, targetSoln.Value, trans);
                SolutionContainerSystem.TryAddSolution(solutionComp.Value, drained);
                _audioSystem.PlayPredicted(entity.Comp.WelderRefill, entity, user: args.User);
                _popup.PopupClient(Loc.GetString("welder-component-after-interact-refueled-message"), entity, args.User);
            }
            else if (welderSolution.AvailableVolume <= 0)
            {
                _popup.PopupClient(Loc.GetString("solution-refuel-component-already-full", ("target", entity)), entity, args.User);
            }
            else
            {
                _popup.PopupClient(Loc.GetString("welder-component-no-fuel-in-tank", ("owner", args.Target)), entity, args.User);
            }

            args.Handled = true;
        }
    }
}
