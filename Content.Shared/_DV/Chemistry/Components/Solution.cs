using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;

namespace Content.Shared.Chemistry.Components;

public sealed partial class Solution
{
    /// <summary>
    /// splits the solution taking up to the specified amount of each reagent from the solution.
    /// If the solution has less of a reagent than the specified amount, it will take all of that reagent.
    /// </summary>
    /// <param name="toTakePer">How much of each reagent to take</param>
    /// <returns>a new solution containing the reagents taken from the original solution</returns>
    public Solution SplitSolutionReagentsEvenly(FixedPoint2 toTakePer)
    {
        var splitSolution = new Solution();

        if (toTakePer <= FixedPoint2.Zero)
            return splitSolution;

        var reagentsCount = Contents.Count;
        var reagentsToRemove = new List<ReagentQuantity>();

        for (var i = 0; i < reagentsCount; i++)
        {
            var currentReagent = Contents[i];

            if (currentReagent.Quantity <= FixedPoint2.Zero)
            {
                reagentsToRemove.Add(currentReagent);
                continue;
            }

            if (currentReagent.Quantity <= toTakePer)
            {
                splitSolution.AddReagent(currentReagent);
                reagentsToRemove.Add(currentReagent);
            }
            else
            {
                splitSolution.AddReagent(currentReagent.Reagent, toTakePer);
                RemoveReagent(currentReagent.Reagent, toTakePer);
            }
        }

        foreach (var reagent in reagentsToRemove)
        {
            RemoveReagent(reagent);
        }

        if (Volume == FixedPoint2.Zero)
            RemoveAllSolution();

        _heatCapacityDirty = true;
        splitSolution._heatCapacityDirty = true;
        ValidateSolution();
        splitSolution.ValidateSolution();

        return splitSolution;
    }
}
