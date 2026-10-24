using Content.Server.Power.EntitySystems;
using Content.Shared._DV.Fabricator;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Fabricator;

public sealed class FabricatorSystem : SharedFabricatorSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

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

            _audio.PlayPvs(fab.DoneSound, uid, AudioParams.Default.WithVolume(-2));
        }
    }

    // TODO refactor this away from being one gigantic fuckoff function
    private void CompleteRecipe(EntityUid uid, FabricatorComponent comp)
    {
        var (recipe, portions) = comp.PortionedRecipe;
        if (recipe is null)
            return;

        // store items we need to remove, and multiply them by portion sizes
        var removeItems = new Dictionary<ProtoId<EntityPrototype>, int>(recipe.Solids);
        var removeReagents = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>(recipe.Reagents);
        var removeMaterials = new Dictionary<ProtoId<MaterialPrototype>, int>(recipe.Materials);

        if (portions > 1)
        {
            foreach (var key in removeItems.Keys)
                removeItems[key] *= portions;
            foreach (var key in removeReagents.Keys)
                removeReagents[key] *= portions;
            foreach (var key in removeMaterials.Keys)
                removeMaterials[key] *= portions;
        }

        // subtract ingredients
        foreach (var item in comp.Storage.ContainedEntities)
        {
            // check reagents first in case we delete the container
            if (_solutionContainer.TryGetDrainableSolution(item, out var solnEnt, out _))
            {
                // find the reagents we want and remove them
                foreach (var (reagent, _) in recipe.Reagents)
                {
                    // skip if not in dict or fulfulled
                    if (!removeReagents.TryGetValue(reagent, out var reagentQuantity) || reagentQuantity <= 0)
                        continue;

                    var removed =
                        _solutionContainer.RemoveReagent(solnEnt.Value, new ReagentId(reagent.Id, null), reagentQuantity);
                    removeReagents[reagent] -= removed;
                }
            }

            // grab stack component now
            StackComponent? stack = null;
            var stackCount = 1;

            if (Resolve(item, ref stack, false))
            {
                stackCount = stack.Count;
            }

            // check materials
            if (TryComp<PhysicalCompositionComponent>(item, out var physicalCompositionComponent)
                && HasComp<MaterialComponent>(item))
            {
                foreach (var (material, perStack) in physicalCompositionComponent.MaterialComposition)
                {
                    // skip if not in dict or fulfulled
                    if (!removeMaterials.TryGetValue(material, out var materialsNeed) || materialsNeed <= 0)
                        continue;

                    // get the actual amount of materials we have
                    var materialsHave = perStack * stackCount;

                    if (materialsNeed > materialsHave)
                    {
                        // we need more, fully deplete this entity
                        removeMaterials[material] -= materialsHave;
                        QueueDel(item);
                        continue;
                    }

                    // ceiling round (no free materials)
                    if (materialsNeed % perStack != 0)
                        materialsNeed += perStack;

                    // we got more than we need
                    if (stack is not null)
                    {
                        _stack.ReduceCount(item, materialsNeed / perStack);
                        Dirty(item, stack);
                    }
                    removeMaterials[material] = 0;
                }

                // all item entities with material properties are skipped
                continue;
            }

            // :(
            var itemMeta = MetaData(item);
            if (itemMeta.EntityPrototype is null)
                continue;
            var itemId = itemMeta.EntityPrototype.ID;

            // skip if item isn't in the recipe or is already fulfilled
            if (!removeItems.ContainsKey(itemId)
                || removeItems[itemId] <= 0)
                continue;

            // ReduceCount doesn't return the amount taken, so we have to do a separate check
            if (removeItems[itemId] >= stackCount)
            {
                removeItems[itemId] -= stackCount;
                QueueDel(item);
                continue;
            }

            // will have leftovers, can set needed to zero
            _stack.ReduceCount(item, removeItems[itemId]);
            removeItems[itemId] = 0;

            if (stack is not null)
                Dirty(item, stack);
        }

        // spawn our results :)
        for (var i = 0; i < portions; ++i)
            Spawn(recipe.Result, Transform(uid).Coordinates);
    }
}
