using System.Linq;
using Content.Shared._DV.Kitchen;
using Content.Shared._DV.Kitchen.Components;
using Content.Shared._DV.Kitchen.Systems;
using Content.Shared.Audio;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Throwing;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Kitchen;

public sealed class DeepFryerSystem : SharedDeepFryerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;

    /// <summary>
    /// The trigger key used when non-frying oil reagents are added to the fryer
    /// </summary>
    public const string WrongReagentTriggerKey = "reaction";

    private readonly List<EntityUid> _itemsToComplete = new();
    private readonly List<EntityUid> _itemsToBurn = new();
    private readonly HashSet<EntityUid> _processedItems = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeepFryerComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<DeepFryerComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
        SubscribeLocalEvent<DeepFryerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<DeepFryerComponent, SolutionTransferredEvent>(OnSolutionTransferred);
        SubscribeLocalEvent<DeepFryerComponent, ThrowHitByEvent>(OnThrowHitBy);
    }

    private void OnSolutionTransferred(Entity<DeepFryerComponent> ent, ref SolutionTransferredEvent args)
    {
        // Only restore quality when oil is being added TO the fryer (not removed from it)
        if (args.To != ent.Owner)
            return;

        // Get the fryer's solution to check what reagents are now in it
        if (Solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var solution))
        {
            // Check if any reagents in the solution are NOT valid frying oils
            var hasInvalidReagent = false;
            foreach (var reagent in solution.Contents)
            {
                if (!ent.Comp.FryingOils.Contains(reagent.Reagent.Prototype))
                {
                    hasInvalidReagent = true;
                    break;
                }
            }

            // If we found an invalid reagent, trigger the reaction
            if (hasInvalidReagent)
            {
                _trigger.Trigger(ent, args.User, WrongReagentTriggerKey);

                // Don't restore oil quality if we're triggering an explosion
                return;
            }
        }

        // Restore oil quality based on the amount transferred
        var qualityRestored = (float)args.Amount * ent.Comp.OilQualityRestorationPerUnit;
        ent.Comp.OilQuality = Math.Min(1.0f, ent.Comp.OilQuality + qualityRestored);
        Dirty(ent);
    }

    private void OnPowerChanged(Entity<DeepFryerComponent> ent, ref PowerChangedEvent args)
    {
        UpdateAppearance(ent);
    }

    private void OnThrowHitBy(Entity<DeepFryerComponent> ent, ref ThrowHitByEvent args)
    {
        if (args.Component.Thrower is not { } thrower || !CanInsertItem(ent, args.Thrown, out _))
            return;

        if (!HasComp<ProfessionalChefComponent>(thrower) && _random.Prob(ent.Comp.MissChance))
        {
            // Item missed! Let it continue with normal throw physics
            Popup.PopupEntity(Loc.GetString("deep-fryer-throw-miss", ("item", args.Thrown)), ent, thrower);
            return;
        }

        // Success! Insert the item
        if (TryInsertItem(ent, args.Thrown, thrower))
            Popup.PopupEntity(Loc.GetString("deep-fryer-throw-success", ("item", args.Thrown)), ent, thrower);
    }

    private void OnItemInserted(Entity<DeepFryerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerName)
            return;

        if (!_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
            return;

        // First, check if this new item completes any multi-ingredient recipes with items already in the fryer
        var completedMultiRecipe = TryFindAndUpgradeToMultiRecipe(ent, container);

        if (completedMultiRecipe == null)
        {
            // No multi-recipe was completed, so assign this item its best single-ingredient recipe
            var singleRecipe = FindBestRecipeForItem(args.Entity);
            ent.Comp.CookingItems[args.Entity] = new CookingItem(singleRecipe, _timing.CurTime);
        }

        UpdateAppearance(ent);
    }

    private void OnItemRemoved(Entity<DeepFryerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        // Only process items in the fryer basket
        if (args.Container.ID != ent.Comp.ContainerName)
            return;

        // Remove from cooking tracking
        ent.Comp.CookingItems.Remove(args.Entity);

        UpdateAppearance(ent);
    }

    /// <summary>
    /// Updates the visual appearance of the deep fryer based on power and cooking state
    /// </summary>
    private void UpdateAppearance(Entity<DeepFryerComponent> ent)
    {
        var isBubbling = false;

        // Check if the fryer is powered and has items
        if (_power.IsPowered(ent.Owner)
            && ent.Comp.CookingItems.Count > 0
            && HasEnoughOil(ent))
        {
            isBubbling = true;
        }

        UpdateAmbience(ent, isBubbling);
        _appearance.SetData(ent, DeepFryerVisuals.Bubbling, isBubbling);
    }

    private void UpdateAmbience(Entity<DeepFryerComponent> ent, bool value)
    {
        _ambientSound.SetAmbience(ent, value);
    }

    /// <summary>
    /// Finds the best recipe for a single item.
    /// Prioritizes multi-ingredient recipes (returns null so item waits), then single-ingredient recipes.
    /// Returns null if item is only in multi-ingredient recipes or has no recipe at all.
    /// </summary>
    private ProtoId<DeepFryerRecipePrototype>? FindBestRecipeForItem(EntityUid item)
    {
        var itemProto = MetaData(item).EntityPrototype?.ID;
        if (itemProto == null)
            return null;

        ProtoId<DeepFryerRecipePrototype>? singleIngredientRecipe = null;

        // Look through all deep fryer recipes
        foreach (var deepFryerRecipe in _prototype.EnumeratePrototypes<DeepFryerRecipePrototype>())
        {
            // Get the base microwave recipe
            if (!_prototype.Resolve(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
                continue;

            // Count total solid ingredients
            FixedPoint2 totalIngredients = 0;
            var hasThisItem = false;

            foreach (var (ingredientId, count) in microwaveRecipe.IngredientsSolids)
            {
                totalIngredients += count;
                if (ingredientId == itemProto)
                    hasThisItem = true;
            }

            if (!hasThisItem)
                continue;

            if (totalIngredients == 1)
            {
                // This is a single-ingredient recipe
                singleIngredientRecipe = deepFryerRecipe.ID;
            }
        }

        // Return the single-ingredient recipe (may be null)
        return singleIngredientRecipe;
    }

    /// <summary>
    /// Checks if the newly inserted item completes any multi-ingredient recipe with existing items.
    /// If so, upgrades all involved items to use that recipe.
    /// Returns the recipe if one was found and upgraded, null otherwise.
    /// </summary>
    private ProtoId<DeepFryerRecipePrototype>? TryFindAndUpgradeToMultiRecipe(
        Entity<DeepFryerComponent> ent,
        BaseContainer container)
    {
        // Look through all multi-ingredient recipes to see if any are now complete
        foreach (var deepFryerRecipe in _prototype.EnumeratePrototypes<DeepFryerRecipePrototype>())
        {
            // Get the base microwave recipe
            if (!_prototype.Resolve(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
                continue;

            // Count total solid ingredients
            FixedPoint2 totalIngredients = 0;
            foreach (var (_, count) in microwaveRecipe.IngredientsSolids)
            {
                totalIngredients += count;
            }

            // Skip single-ingredient recipes
            if (totalIngredients <= 1)
                continue;

            // Check if all ingredients for this multi-ingredient recipe are present
            var ingredients = GetIngredientsForRecipe(deepFryerRecipe.ID, container);
            if (ingredients == null)
                continue;

            // Check if ingredients are within tolerance
            if (!AreIngredientsWithinTolerance(ent, ingredients))
                continue;

            // We found a complete multi-ingredient recipe within tolerance!
            // Upgrade all ingredients to use this recipe

            // Find the earliest start time among all ingredients
            var earliestTime = _timing.CurTime;
            foreach (var (ingredientUid, _) in ingredients)
            {
                if (ent.Comp.CookingItems.TryGetValue(ingredientUid, out var existingItem))
                {
                    if (existingItem.TimeStarted < earliestTime)
                        earliestTime = existingItem.TimeStarted;
                }
            }

            // Assign the multi-ingredient recipe to all ingredients with synchronized start time
            foreach (var (ingredientUid, _) in ingredients)
            {
                ent.Comp.CookingItems[ingredientUid] = new CookingItem(deepFryerRecipe.ID, earliestTime);
            }

            return deepFryerRecipe.ID;
        }

        return null;
    }

    /// <summary>
    /// Tries to get all ingredients needed for a specific recipe from the fryer.
    /// Returns null if not all ingredients are present.
    /// </summary>
    private Dictionary<EntityUid, string>? GetIngredientsForRecipe(
        ProtoId<DeepFryerRecipePrototype> recipeId,
        BaseContainer container)
    {
        if (!_prototype.TryIndex(recipeId, out var deepFryerRecipe))
            return null;

        if (!_prototype.Resolve(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
            return null;

        var neededIngredients = new Dictionary<string, FixedPoint2>();
        foreach (var (ingredient, count) in microwaveRecipe.IngredientsSolids)
        {
            neededIngredients[ingredient] = count;
        }

        var foundIngredients = new Dictionary<EntityUid, string>();

        // Check each item in the fryer
        foreach (var itemUid in container.ContainedEntities)
        {
            var itemProto = MetaData(itemUid).EntityPrototype?.ID;
            if (itemProto == null)
                continue;

            // If this item is one of the needed ingredients
            if (neededIngredients.TryGetValue(itemProto, out var needed) && needed > 0)
            {
                foundIngredients[itemUid] = itemProto;
                neededIngredients[itemProto] -= 1;
            }
        }

        // Check if we found all required ingredients
        foreach (var (_, count) in neededIngredients)
        {
            if (count > 0)
                return null; // Missing some ingredients
        }

        return foundIngredients;
    }

    /// <summary>
    /// Checks if all ingredients for a multi-ingredient recipe are within the cooking time tolerance
    /// </summary>
    private bool AreIngredientsWithinTolerance(
        Entity<DeepFryerComponent> ent,
        Dictionary<EntityUid, string> ingredients)
    {
        if (ingredients.Count <= 1)
            return true;

        TimeSpan? earliest = null;
        TimeSpan? latest = null;

        foreach (var (ingredientUid, _) in ingredients)
        {
            if (!ent.Comp.CookingItems.TryGetValue(ingredientUid, out var cookingItem))
                continue;

            if (earliest == null || cookingItem.TimeStarted < earliest)
                earliest = cookingItem.TimeStarted;

            if (latest == null || cookingItem.TimeStarted > latest)
                latest = cookingItem.TimeStarted;
        }

        if (earliest == null || latest == null)
            return false;

        var timeDifference = latest.Value - earliest.Value;
        return timeDifference <= ent.Comp.CookingTolerance;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _itemsToComplete.Clear();
        _itemsToBurn.Clear();

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<DeepFryerComponent>();

        while (query.MoveNext(out var uid, out var fryer))
        {
            // Skip if not powered
            if (!_power.IsPowered(uid))
                continue;

            // Skip if no oil
            if (!HasEnoughOil((uid, fryer)))
                continue;

            // Get the container
            if (!_container.TryGetContainer(uid, fryer.ContainerName, out var container))
                continue;

            _processedItems.Clear();

            // Process each cooking item
            foreach (var (itemUid, cookingItem) in fryer.CookingItems.ToList())
            {
                // Skip if already processed as part of a multi-ingredient recipe
                if (_processedItems.Contains(itemUid))
                    continue;

                var elapsedTime = curTime - cookingItem.TimeStarted;

                // If the item is already marked as burning
                if (cookingItem.IsBurning)
                {
                    if (cookingItem.Recipe is { } burningRecipe)
                    {
                        if (!_prototype.TryIndex(burningRecipe, out var deepFryerRecipe))
                            continue;

                        var burnTime = deepFryerRecipe.BurnTime;
                        if (elapsedTime >= burnTime)
                        {
                            _itemsToBurn.Add(itemUid);
                        }
                    }
                    continue;
                }

                // Item is cooking (not burning yet)
                if (cookingItem.Recipe is { } recipe)
                {
                    if (!_prototype.TryIndex(recipe, out var deepFryerRecipe))
                        continue;

                    if (!_prototype.Resolve(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
                        continue;

                    var cookTime = TimeSpan.FromSeconds(microwaveRecipe.CookTime);

                    // Check if this is part of a multi-ingredient recipe
                    var multiIngredients = GetIngredientsForRecipe(recipe, container);

                    if (multiIngredients is { Count: > 1 })
                    {
                        // This is a multi-ingredient recipe
                        // Check if all ingredients are still within tolerance
                        if (!AreIngredientsWithinTolerance((uid, fryer), multiIngredients))
                            continue;

                        // Find the earliest start time
                        var earliestStart = TimeSpan.MaxValue;
                        foreach (var (ingredientUid, _) in multiIngredients)
                        {
                            // TryGetValue in Update my beloved
                            // Only like one deep fryer per map so it's gonna be fine probably
                            if (!fryer.CookingItems.TryGetValue(ingredientUid, out var ingredientCookingItem))
                                continue;

                            if (ingredientCookingItem.TimeStarted < earliestStart)
                                earliestStart = ingredientCookingItem.TimeStarted;
                        }

                        // Check if enough time has passed since the earliest ingredient
                        var earliestElapsed = curTime - earliestStart;
                        if (earliestElapsed < cookTime)
                            continue;
                        {
                            // Mark the first item for completion (it will handle all ingredients)
                            _itemsToComplete.Add(itemUid);
                            // Mark all ingredients as processed
                            foreach (var (ingredientUid, _) in multiIngredients)
                            {
                                _processedItems.Add(ingredientUid);
                            }
                        }
                        // Note: If ingredients are outside tolerance, they keep their current recipes
                        // and will be handled individually (single-ingredient recipes will complete, items without recipes will burn)
                    }
                    else
                    {
                        // Single-ingredient recipe, proceed normally
                        if (elapsedTime >= cookTime)
                        {
                            _itemsToComplete.Add(itemUid);
                        }
                    }
                }
                else
                {
                    // Item has no recipe assigned - it should burn after BaseBurnTime
                    if (elapsedTime >= fryer.BaseBurnTime)
                    {
                        _itemsToBurn.Add(itemUid);
                    }
                }
            }

            // Complete cooking for finished items
            foreach (var itemUid in _itemsToComplete)
            {
                CompleteCooking((uid, fryer), itemUid, container);
            }

            // Burn items that have been cooking too long or have no recipe
            foreach (var itemUid in _itemsToBurn)
            {
                BurnItem((uid, fryer), itemUid, container);
            }
        }
    }

    /// <summary>
    /// Completes cooking for an item (or multi-ingredient recipe), transforming it into the result
    /// </summary>
    private void CompleteCooking(Entity<DeepFryerComponent> ent, EntityUid item, BaseContainer container)
    {
        if (!ent.Comp.CookingItems.TryGetValue(item, out var cookingItem))
            return;

        if (cookingItem.Recipe is not { } recipe)
            return;

        if (!_prototype.TryIndex(recipe, out var deepFryerRecipe))
            return;

        // Get the base microwave recipe for result
        if (!_prototype.Resolve(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
            return;

        // Get all ingredients for this recipe
        var recipeIngredients = GetIngredientsForRecipe(recipe, container);
        var isMultiIngredient = recipeIngredients is { Count: > 1 };

        // Check if we should burn the item due to foul oil
        var qualityLevel = GetOilQualityLevel(ent.Comp.OilQuality);
        if (qualityLevel == OilQuality.Foul && _random.Prob(ent.Comp.FoulOilBurnChance))
        {
            // For multi-ingredient recipes, burn all ingredients
            if (isMultiIngredient)
            {
                foreach (var (ingredientUid, _) in recipeIngredients!)
                {
                    BurnItem(ent, ingredientUid, container, recipe: recipe);
                }
                return;
            }

            // Force burn the single item
            BurnItem(ent, item, container, recipe: recipe);
            return;
        }

        var xform = Transform(ent);
        var coords = Xform.GetMapCoordinates((ent, xform));

        // For multi-ingredient recipes, remove ALL ingredients
        if (isMultiIngredient)
        {
            // Delete all ingredients
            foreach (var (ingredientUid, _) in recipeIngredients!)
            {
                ent.Comp.CookingItems.Remove(ingredientUid);
                _container.Remove(ingredientUid, container);
                QueueDel(ingredientUid);
            }
        }
        else
        {
            // Single ingredient recipe
            ent.Comp.CookingItems.Remove(item);
            _container.Remove(item, container);
            QueueDel(item);
        }

        // Spawn the result (from the microwave recipe)
        var result = Spawn(microwaveRecipe.Result, coords);

        // Transfer solution from fryer to food (includes oil AND any contaminants!)
        TransferOilToFood(ent, result, deepFryerRecipe.OilConsumption);

        // Add flavors based on oil quality
        AddOilQualityFlavors(result, ent.Comp, qualityLevel);

        // Degrade oil quality
        DegradeOilQuality(ent);

        // Try to put it back in the fryer
        if (!_container.Insert(result, container))
        {
            // If we can't insert it (container full?), just leave it at the fryer's location
            Xform.SetCoordinates(result, xform, Xform.GetMoverCoordinates(ent, xform));
        }
        else
        {
            // Track the result and start burning timer
            ent.Comp.CookingItems[result] = new CookingItem(cookingItem.Recipe, _timing.CurTime, isBurning: true);
        }

        // Show a popup
        Popup.PopupEntity(Loc.GetString("deep-fryer-item-finished", ("item", result)), ent, PopupType.Medium);
        _audio.PlayPvs(ent.Comp.FinishedCookingSound, ent);
    }

    /// <summary>
    /// Burns an item - uses recipe's BurnedResult if available, otherwise uses BaseBurnedResult
    /// </summary>
    private void BurnItem(Entity<DeepFryerComponent> ent, EntityUid item, BaseContainer container, ProtoId<DeepFryerRecipePrototype>? recipe = null)
    {

        EntProtoId? burnedEntity = null;
        // If we were never explicitly given a recipe, then see if there's one
        if (!recipe.HasValue && ent.Comp.CookingItems.TryGetValue(item, out var cookingItem) && cookingItem.Recipe is { } foundRecipe)
            recipe = foundRecipe;

        // Try to get the recipe, if we were given one or if we found one
        if (_prototype.TryIndex(recipe, out var deepFryerRecipe))
            burnedEntity = deepFryerRecipe.BurnedResult;

        // finally, if we don't have a BurnedResult from a recipe, just default to BaseBurnedResult
        if (!burnedEntity.HasValue)
            burnedEntity = ent.Comp.BaseBurnedResult;

        // Remove the item from tracking and container
        ent.Comp.CookingItems.Remove(item);
        _container.Remove(item, container);

        // Delete the original item
        QueueDel(item);

        // Spawn the burned result on top of the fryer
        Spawn(burnedEntity, Xform.GetMoverCoordinates(ent));

        // Degrade oil quality even when burning
        DegradeOilQuality(ent);

        // Show a danger popup
        Popup.PopupEntity(Loc.GetString("deep-fryer-item-burned", ("item", item)), ent, PopupType.MediumCaution);
        _audio.PlayPvs(ent.Comp.FinishedBurningSound, ent);
    }

    /// <summary>
    /// Adds flavors to the cooked item based on the current oil quality
    /// </summary>
    private void AddOilQualityFlavors(EntityUid result, DeepFryerComponent fryer, OilQuality qualityLevel)
    {
        // Get or create the FlavorProfile component
        var flavorProfile = EnsureComp<FlavorProfileComponent>(result);

        // Get the flavors for this quality level
        if (!fryer.OilQualityFlavors.TryGetValue(qualityLevel, out var flavors))
            return;

        // Add each flavor to the profile
        foreach (var flavor in flavors)
        {
            flavorProfile.Flavors.Add(flavor);
        }

        Dirty(result, flavorProfile);
    }

    /// <summary>
    /// Degrades the oil quality after cooking an item
    /// </summary>
    private void DegradeOilQuality(Entity<DeepFryerComponent> ent)
    {
        // Calculate degradation multiplier based on oil volume
        var degradationMultiplier = CalculateOilDegradationMultiplier(ent);

        // Reduce oil quality with the multiplier applied
        var degradationAmount = ent.Comp.OilDegradationPerRecipe * degradationMultiplier;
        ent.Comp.OilQuality = Math.Max(0f, ent.Comp.OilQuality - degradationAmount);

        // Mark as dirty to sync to clients
        Dirty(ent);
    }

    /// <summary>
    /// Transfers solution from the fryer into the food solution.
    /// Transfers ALL reagents proportionally - if someone added bleach to the fryer, enjoy your bleach-fried food!
    /// </summary>
    private void TransferOilToFood(Entity<DeepFryerComponent> ent, EntityUid food, FixedPoint2 amount)
    {
        // Get the fryer's solution
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var fryerSolution))
            return;

        // Get the food solution
        if (!Solution.TryGetSolution(food, "food", out var foodSolutionEnt, out _))
            return;

        // Split the desired amount from the fryer - this takes ALL reagents proportionally!
        // If the fryer has 80% oil and 20% bleach, the food gets 80% oil and 20% bleach too!
        var transferredSolution = fryerSolution.SplitSolution(amount);

        // Add the split solution to the food
        Solution.AddSolution(foodSolutionEnt.Value, transferredSolution);
    }
}
