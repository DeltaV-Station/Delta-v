using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;

namespace Content.Shared.Chemistry.EntitySystems;

public abstract partial class SharedSolutionContainerSystem
{
    /// <summary>
    /// Splits a solution removing a specified amount of each reagent, if available.
    /// </summary>
    /// <param name="soln">The container to split the solution from.</param>
    /// <param name="quantity">The amount of each reagent to split.</param>
    /// <returns></returns>
    public Solution SplitSolutionReagentsEvenly(Entity<SolutionComponent> soln, FixedPoint2 quantity)
    {
        var (uid, comp) = soln;
        var solution = comp.Solution;

        var splitSol = solution.SplitSolutionReagentsEvenly(quantity);
        UpdateChemicals(soln);
        return splitSol;
    }
}
