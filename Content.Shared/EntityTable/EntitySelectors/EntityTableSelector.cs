using Content.Shared.EntityTable.ValueSelector;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.EntityTable.EntitySelectors;

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class EntityTableSelector
{
    /// <summary>
    /// The number of times this selector is run
    /// </summary>
    [DataField]
    public NumberSelector Rolls = new ConstantNumberSelector(1);

    /// <summary>
    /// A weight used to pick between selectors.
    /// </summary>
    [DataField]
    public float Weight = 1;

    /// <summary>
    /// A simple chance that the selector will run.
    /// </summary>
    [DataField]
    public double Prob = 1;

    public IEnumerable<EntProtoId> GetSpawns(System.Random rand,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
<<<<<<< HEAD
        var rolls = Rolls.Get(rand, entMan, proto);
=======
        if (!CheckConditions(entMan, proto, ctx))
            yield break;

        var rolls = Rolls.Get(rand);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        for (var i = 0; i < rolls; i++)
        {
            if (!rand.Prob(Prob))
                continue;

            foreach (var spawn in GetSpawnsImplementation(rand, entMan, proto, ctx))
            {
                yield return spawn;
            }
        }
    }

<<<<<<< HEAD
=======
    public bool CheckConditions(IEntityManager entMan, IPrototypeManager proto, EntityTableContext ctx)
    {
        if (Conditions.Count == 0)
            return true;

        var success = false;
        foreach (var condition in Conditions)
        {
            var res = condition.Evaluate(this, entMan, proto, ctx);

            if (RequireAll && !res)
                return false; // intentional break out of loop and function

            success |= res;
        }

        if (RequireAll)
            return true;

        return success;
    }

>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    protected abstract IEnumerable<EntProtoId> GetSpawnsImplementation(System.Random rand,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx);
}
