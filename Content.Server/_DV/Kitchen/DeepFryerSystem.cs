using Content.Server.Power.Components;
using Content.Shared._DV.Kitchen;
using Content.Shared._DV.Kitchen.Components;
using Content.Shared._DV.Kitchen.Systems;
using Content.Shared.Audio;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
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

    private readonly List<EntityUid> _itemsToComplete = new();
    private readonly List<EntityUid> _itemsToBurn = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeepFryerComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<DeepFryerComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
        SubscribeLocalEvent<DeepFryerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<DeepFryerComponent, SolutionTransferredEvent>(OnSolutionTransferred);
    }

    private void OnSolutionTransferred(Entity<DeepFryerComponent> ent, ref SolutionTransferredEvent args)
    {
        // Only restore quality when oil is being added TO the fryer (not removed from it)
        if (args.To != ent.Owner)
            return;

        // Restore oil quality based on the amount transferred
        var qualityRestored = (float)args.Amount * ent.Comp.OilQualityRestorationPerUnit;
        ent.Comp.OilQuality = Math.Min(1.0f, ent.Comp.OilQuality + qualityRestored);
        Dirty(ent);
    }

    private void OnPowerChanged(Entity<DeepFryerComponent> ent, ref PowerChangedEvent args)
    {
        UpdateAppearance(ent);
    }

    private void OnItemInserted(Entity<DeepFryerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerName)
            return;

        // Try to find a matching deep fryer recipe for this item
        var recipe = FindMatchingRecipe(args.Entity);

        // Add to cooking items tracking with current time as start time
        ent.Comp.CookingItems[args.Entity] = new CookingItem(recipe, _timing.CurTime);

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
        if (_power.IsPowered(ent.Owner))
        {
            if (ent.Comp.CookingItems.Count > 0 && HasEnoughOil(ent))
            {
                isBubbling = true;
            }
        }

        UpdateAmbience(ent, isBubbling);
        _appearance.SetData(ent, DeepFryerVisuals.Bubbling, isBubbling);
    }

    private void UpdateAmbience(Entity<DeepFryerComponent> ent, bool value)
    {
        _ambientSound.SetAmbience(ent, value);
    }

    /// <summary>
    /// Finds a deep fryer recipe that matches the given item
    /// </summary>
    private ProtoId<DeepFryerRecipePrototype>? FindMatchingRecipe(EntityUid item)
    {
        var itemProto = MetaData(item).EntityPrototype?.ID;
        if (itemProto == null)
            return null;

        // Look through all deep fryer recipes
        foreach (var deepFryerRecipe in _prototype.EnumeratePrototypes<DeepFryerRecipePrototype>())
        {
            // Get the base microwave recipe
            if (!_prototype.TryIndex(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
                continue;

            // Check if this recipe uses our item as a solid ingredient
            foreach (var solid in microwaveRecipe.IngredientsSolids)
            {
                if (solid.Key == itemProto && solid.Value == 1)
                {
                    return deepFryerRecipe.ID;
                }
            }
        }

        return null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DeepFryerComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var fryer, out var power))
        {
            // Only cook if powered
            if (!_power.IsPowered((uid, power)))
                continue;

            // Skip if no items cooking
            if (fryer.CookingItems.Count == 0)
                continue;

            // Check if we have enough oil using the shared method
            if (!HasEnoughOil((uid, fryer)))
                continue;

            // Get the container
            if (!_container.TryGetContainer(uid, fryer.ContainerName, out var container))
                continue;

            // Process each cooking item
            _itemsToComplete.Clear();
            _itemsToBurn.Clear();
            var curTime = _timing.CurTime;

            foreach (var (itemUid, cookingItem) in fryer.CookingItems)
            {
                // Skip items without recipes
                if (cookingItem.Recipe == null)
                    continue;

                // Get the deep fryer recipe
                if (!_prototype.TryIndex(cookingItem.Recipe.Value, out var deepFryerRecipe))
                    continue;

                // Calculate elapsed time
                var elapsedTime = curTime - cookingItem.TimeStarted;

                if (cookingItem.IsBurning)
                {
                    // Check if burning is complete
                    if (elapsedTime >= deepFryerRecipe.BurnTime)
                    {
                        _itemsToBurn.Add(itemUid);
                    }
                }
                else
                {
                    // Get the base microwave recipe for timing
                    if (!_prototype.TryIndex(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
                        continue;

                    // Check if cooking is complete
                    if (elapsedTime >= TimeSpan.FromSeconds(microwaveRecipe.CookTime))
                    {
                        _itemsToComplete.Add(itemUid);

                    }
                }
            }

            // Complete cooking for finished items
            foreach (var itemUid in _itemsToComplete)
            {
                CompleteCooking((uid, fryer), itemUid, container);
            }

            // Burn items that have been cooking too long
            foreach (var itemUid in _itemsToBurn)
            {
                CompleteBurning((uid, fryer), itemUid, container);
            }
        }
    }

    /// <summary>
    /// Completes cooking for an item, transforming it into the result
    /// </summary>
    private void CompleteCooking(Entity<DeepFryerComponent> ent, EntityUid item, BaseContainer container)
    {
        if (!ent.Comp.CookingItems.TryGetValue(item, out var cookingItem))
            return;

        if (cookingItem.Recipe == null)
            return;

        if (!_prototype.TryIndex(cookingItem.Recipe.Value, out var deepFryerRecipe))
            return;

        // Get the base microwave recipe for result
        if (!_prototype.TryIndex(deepFryerRecipe.BaseRecipe, out var microwaveRecipe))
            return;

        // Check if we should burn the item due to foul oil
        var qualityLevel = GetOilQualityLevel(ent.Comp.OilQuality);
        if (qualityLevel == OilQuality.Foul && _random.Prob(ent.Comp.FoulOilBurnChance))
        {
            // Force burn the item
            CompleteBurning(ent, item, container);
            return;
        }

        var xform = Transform(ent);

        var coords = Xform.GetMapCoordinates((ent, xform));

        // Remove the item from tracking and container
        ent.Comp.CookingItems.Remove(item);
        _container.Remove(item, container);

        // Delete the original item
        QueueDel(item);

        // Spawn the result (from the microwave recipe)
        var result = Spawn(microwaveRecipe.Result, coords);

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
        Popup.PopupEntity(Loc.GetString("deep-fryer-item-finished", ("item", item)), ent, PopupType.Medium);
        _audio.PlayPvs(ent.Comp.FinishedCookingSound, ent);
    }

    /// <summary>
    /// Burns an item that has been left in the fryer too long
    /// </summary>
    private void CompleteBurning(Entity<DeepFryerComponent> ent, EntityUid item, BaseContainer container)
    {
        if (!ent.Comp.CookingItems.TryGetValue(item, out var cookingItem))
            return;

        if (cookingItem.Recipe == null)
            return;

        if (!_prototype.TryIndex(cookingItem.Recipe.Value, out var deepFryerRecipe))
            return;

        // Remove the item from tracking and container
        ent.Comp.CookingItems.Remove(item);
        _container.Remove(item, container);

        // Delete the original item
        QueueDel(item);

        // Spawn the burned result on top of the fryer
        Spawn(deepFryerRecipe.BurnedResult, Xform.GetMoverCoordinates(ent));

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
}
