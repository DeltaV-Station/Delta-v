using System.Linq;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Kitchen.Components;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Fabricator;

public abstract class SharedFabricatorSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private List<FabricatorRecipePrototype> Recipes = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FabricatorComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<FabricatorComponent, InteractUsingEvent>(OnInteractUsing, after: [typeof(AnchorableSystem)]);

        SubscribeLocalEvent<FabricatorComponent, SolutionContainerChangedEvent>(OnContentsChanged);
        SubscribeLocalEvent<FabricatorComponent, EntInsertedIntoContainerMessage>(OnContentsChanged);
        SubscribeLocalEvent<FabricatorComponent, EntRemovedFromContainerMessage>(OnContentsChanged);

        SubscribeLocalEvent<FabricatorComponent, FabricatorStartMessage>(OnStartMessage);
        SubscribeLocalEvent<FabricatorComponent, FabricatorEjectMessage>(OnEjectMessage);
        SubscribeLocalEvent<FabricatorComponent, FabricatorEjectIndexedMessage>(OnEjectIndexedMessage);

        // TODO cancel fab process on reaction
        // SubscribeLocalEvent<ActivelyMicrowavedComponent, SolutionRelayEvent<ReactionAttemptEvent>>(OnReactionAttempt);

        InitializeRecipes();

    }

    private void InitializeRecipes()
    {
        foreach (var recipe in _prototypeManager.EnumeratePrototypes<FabricatorRecipePrototype>())
        {
            Recipes.Add(recipe);
        }

        Recipes.Sort(new FabricatorRecipePrototype.FabricatorRecipeComparer());
    }

    public void Stop(EntityUid uid, FabricatorComponent comp)
    {
        _audio.Stop(comp.SoundStream);
        comp.FabRunning = false;
        comp.PortionedRecipe = (null, 0);
        UpdateUserInterfaceState((uid, comp));
        Dirty(uid, comp);
    }

    private void UpdateUserInterfaceState(Entity<FabricatorComponent> ent)
    {
        List<(NetEntity, int)> solids = new();
        foreach (var contained in ent.Comp.Storage.ContainedEntities)
        {
            var stackSize = 1;
            if (TryComp<StackComponent>(contained, out var stackComp))
                stackSize = stackComp.Count;
            solids.Add((GetNetEntity(contained), stackSize));

        }

        _userInterface.SetUiState(ent.Owner, MicrowaveUiKey.Key, new FabricatorUpdateUserInterfaceState(
            solids.ToArray(),
            GetReagents(ent),
            ent.Comp.FabRunning,
            TimeSpan.Zero));
    }

    /// <summary>
    /// Gets reagents by id, combining duplicates in different containers
    /// </summary>
    private Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> GetReagents(Entity<FabricatorComponent> ent)
    {
        Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> reagents = new();
        foreach (var item in ent.Comp.Storage.ContainedEntities)
        {
            if (!_solutionContainer.TryGetDrainableSolution(item, out _, out var solution))
                continue;

            foreach (var reagent in solution.Contents)
            {
                if (!reagents.TryAdd(reagent.Reagent.Prototype, reagent.Quantity))
                    reagents[reagent.Reagent.Prototype] += reagent.Quantity;
            }
        }
        return reagents;
    }

    /// <summary>
    /// Gets solid items by id, combining duplicates in different stacks
    /// </summary>
    private Dictionary<ProtoId<EntityPrototype>, int> GetSolids(Entity<FabricatorComponent> ent)
    {
        Dictionary<ProtoId<EntityPrototype>, int> solids = new();

        foreach (var item in ent.Comp.Storage.ContainedEntities)
        {
            var stack = 1;
            if (TryComp<StackComponent>(item, out var stackComp))
                stack = stackComp.Count;

            var itemMeta = MetaData(item);
            if (itemMeta.EntityPrototype is null)
                continue;

            var itemId = itemMeta.EntityPrototype.ID;

            if (!solids.TryAdd(itemId, stack))
                solids[itemId] += stack;
        }

        return solids;
    }

    private int CanSatisfyRecipe(Entity<FabricatorComponent> ent, FabricatorRecipePrototype proto)
    {
        if (!ent.Comp.RecipeGroups.Contains(proto.Group))
            return 0;

        var portions = ent.Comp.MaxPerBatch;

        var solids = GetSolids(ent);
        foreach (var ingredient in proto.Solids)
        {
            if (!solids.TryGetValue(ingredient.Key, out var solidStack))
                return 0;

            if (solidStack < ingredient.Value)
                return 0;

            portions = Math.Min(portions,
                solidStack / ingredient.Value);
        }

        var reagents = GetReagents(ent);
        foreach (var ingredient in proto.Reagents)
        {
            if (!reagents.TryGetValue(ingredient.Key, out var reagentUnits))
                return 0;

            if (reagentUnits < ingredient.Value)
                return 0;

            portions = Math.Min(portions,
                reagentUnits.Int() / ingredient.Value.Int());
        }

        return portions;
    }

    private void OnInteractUsing(Entity<FabricatorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ItemComponent>(args.Used, out var item))
            return;

        // TODO popups?
        if (ent.Comp.FabRunning)
            return;

        // TODO popups?
        if (_item.GetSizePrototype(item.Size) > _item.GetSizePrototype(ent.Comp.MaxItemSize))
            return;

        // TODO max items in fab
        // TODO check if we have power

        args.Handled = true;
        _hands.TryDropIntoContainer(args.User, args.Used, ent.Comp.Storage);

        UpdateUserInterfaceState(ent);
        _audio.PlayPredicted(ent.Comp.AddItemSound, ent, args.User, AudioParams.Default.WithVolume(-2));

        Dirty(ent);
    }

    private void OnContentsChanged<TEvent>(Entity<FabricatorComponent> ent, ref TEvent args)
    {
       UpdateUserInterfaceState(ent);
    }

    private void OnComponentInit(Entity<FabricatorComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Storage = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
        Dirty(ent);
    }

    private void OnStartMessage(Entity<FabricatorComponent> ent, ref FabricatorStartMessage args)
    {
        if (ent.Comp.FabRunning)
            return;

        foreach (var recipe in Recipes)
        {
            var portions = CanSatisfyRecipe(ent, recipe);
            if (portions <= 0)
                continue;

            ent.Comp.FabRunning = true;
            ent.Comp.SecondsRemaining = (recipe.CompletionTime.Seconds * portions * ent.Comp.TimeMultiplier);
            ent.Comp.PortionedRecipe = (recipe, portions);

            // TODO set power working state/visuals

            UpdateUserInterfaceState(ent);
            _audio.PlayPredicted(ent.Comp.StartSound, ent, args.Actor, AudioParams.Default.WithVolume(-2));
            Dirty(ent);

            return;
        }
    }

    private void OnEjectMessage(Entity<FabricatorComponent> ent, ref FabricatorEjectMessage args)
    {
        if (ent.Comp.FabRunning)
            return;

        _container.EmptyContainer(ent.Comp.Storage);
        UpdateUserInterfaceState(ent);
        _audio.PlayPredicted(ent.Comp.RemoveItemSound, ent, args.Actor, AudioParams.Default.WithVolume(-2));
        Dirty(ent);
    }

    private void OnEjectIndexedMessage(Entity<FabricatorComponent> ent, ref FabricatorEjectIndexedMessage args)
    {
        if (ent.Comp.FabRunning)
            return;

        _container.Remove(GetEntity(args.Target), ent.Comp.Storage);
        UpdateUserInterfaceState(ent);
        _audio.PlayPredicted(ent.Comp.RemoveItemSound, ent, args.Actor, AudioParams.Default.WithVolume(-2));
        Dirty(ent);
    }
}
