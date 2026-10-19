using Content.Shared._DV.Fabricator;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Fabricator;

public sealed class FabricatorSystem : SharedFabricatorSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FabricatorComponent>();
        while (query.MoveNext(out var uid, out var fab))
        {
            if (!fab.FabRunning)
                continue;

            fab.SecondsRemaining -= frameTime;

            if (fab.SecondsRemaining > 0)
                continue;

            CompleteRecipe(uid, fab);

            Stop(uid, fab);
        }
    }

    private void CompleteRecipe(EntityUid uid, FabricatorComponent comp)
    {
        var (recipe, portions) = comp.PortionedRecipe;
        if (recipe is null)
            return;

        // store items we need to remove, and multiply them by portion sizes
        var removeItems = new Dictionary<ProtoId<EntityPrototype>, int>(recipe.Solids);
        foreach (var key in removeItems.Keys)
            removeItems[key] *= portions;

        var removeReagents = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>(recipe.Reagents);
        foreach (var key in removeReagents.Keys)
            removeReagents[key] *= portions;

        // subtract ingredients
        foreach (var item in comp.Storage.ContainedEntities)
        {
            // check reagents first in case we delete the container
            if (_solutionContainer.TryGetDrainableSolution(item, out var solnEnt, out _))
            {
                // find the reagents we want and remove them
                foreach (var (reagent, _) in recipe.Reagents)
                {
                    var removed =
                        _solutionContainer.RemoveReagent(solnEnt.Value, new ReagentId(reagent.Id, null), removeReagents[reagent]);
                    removeReagents[reagent] -= removed;
                }
            }

            // :(
            var itemMeta = MetaData(item);
            if (itemMeta.EntityPrototype is null)
                continue;
            var itemId = itemMeta.EntityPrototype.ID;

            // skip if item isn't in the recipe
            if (!recipe.Solids.ContainsKey(itemId)
                || !removeItems.ContainsKey(itemId)
                || removeItems[itemId] <= 0)
                continue;

            if (!TryComp<StackComponent>(item, out var stack))
            {
                // no stack, just get rid of the item
                QueueDel(item);
                --removeItems[itemId];
                continue;
            }

            // ReduceCount doesn't return the amount taken, so we have to do a separate check
            if (removeItems[itemId] >= stack.Count)
            {
                removeItems[itemId] -= stack.Count;
                QueueDel(item);
                continue;
            }

            // will have leftovers, can set needed to zero
            _stack.ReduceCount(item, removeItems[itemId]);
            removeItems[itemId] = 0;
            Dirty(item, stack);
        }

        // spawn our results :)
        for (var i = 0; i < portions; ++i)
            PredictedSpawnAtPosition(recipe.Result, Transform(uid).Coordinates);
    }
}
