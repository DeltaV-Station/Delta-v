using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Stacks;
using Content.Shared.Wires;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

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
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedPowerStateSystem _powerState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    private List<FabricatorRecipePrototype> Recipes = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FabricatorComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<FabricatorComponent, InteractUsingEvent>(OnInteractUsing,
            after: [typeof(AnchorableSystem), typeof(SharedWiresSystem)]);

        SubscribeLocalEvent<FabricatorComponent, SolutionContainerChangedEvent>(OnContentsChanged);
        SubscribeLocalEvent<FabricatorComponent, EntInsertedIntoContainerMessage>(OnContentsChanged);
        SubscribeLocalEvent<FabricatorComponent, EntRemovedFromContainerMessage>(OnContentsChanged);

        SubscribeLocalEvent<FabricatorComponent, FabricatorStartMessage>(OnStartMessage);
        SubscribeLocalEvent<FabricatorComponent, FabricatorEjectMessage>(OnEjectMessage);
        SubscribeLocalEvent<FabricatorComponent, FabricatorEjectIndexedMessage>(OnEjectIndexedMessage);

        // TODO cancel fab process on reaction
        // SubscribeLocalEvent<ActivelyMicrowavedComponent, SolutionRelayEvent<ReactionAttemptEvent>>(OnReactionAttempt);
        SubscribeLocalEvent<FabricatorComponent, AnchorStateChangedEvent>(OnAnchorStateChanged);
        SubscribeLocalEvent<FabricatorComponent, PowerChangedEvent>(OnPowerChanged);

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

        _powerState.SetWorkingState(uid, false);

        SetAppearance(uid, FabricatorVisualState.Idle, true);
        SetAppearance(uid, FabricatorVisualState.Running, false);

        comp.EndTime = _timing.CurTime;
        comp.SecondsRemaining = 0;

        UpdateUserInterfaceState(uid, comp);

        Dirty(uid, comp);
    }

    private void UpdateUserInterfaceState(EntityUid uid, FabricatorComponent? fabricatorComponent = null)
    {
        if (!Resolve(uid, ref fabricatorComponent, false))
            return;

        if (fabricatorComponent.LifeStage < ComponentLifeStage.Initialized
            || fabricatorComponent.LifeStage >= ComponentLifeStage.Stopping)
            return;

        List<(NetEntity, int)> solids = new();
        foreach (var contained in fabricatorComponent.Storage.ContainedEntities)
        {
            var stackSize = 1;
            if (TryComp<StackComponent>(contained, out var stackComp))
                stackSize = stackComp.Count;
            solids.Add((GetNetEntity(contained), stackSize));
        }

        _userInterface.SetUiState(uid, FabricatorUiKey.Key, new FabricatorUpdateUserInterfaceState(
            solids.ToArray(),
            GetReagents(fabricatorComponent),
            fabricatorComponent.FabRunning,
            fabricatorComponent.EndTime));
    }

    private void SetAppearance(EntityUid uid, FabricatorVisualState state, bool stateVal, AppearanceComponent? appearanceComponent = null)
    {
        if (!Resolve(uid, ref appearanceComponent, false))
            return;
        _appearance.SetData(uid, state, stateVal, appearanceComponent);
    }

    /// <summary>
    /// Gets reagents by id, combining duplicates in different containers
    /// </summary>
    private Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> GetReagents(FabricatorComponent comp)
    {
        Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> reagents = new();
        foreach (var item in comp.Storage.ContainedEntities)
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
            if (HasComp<PhysicalCompositionComponent>(ent) && HasComp<MaterialComponent>(ent))
                continue;

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

    /// <summary>
    /// Gets material amount by id, combining duplicates in different stacks
    /// </summary>
    private Dictionary<ProtoId<MaterialPrototype>, int> GetMaterials(Entity<FabricatorComponent> ent)
    {
        Dictionary<ProtoId<MaterialPrototype>, int> materials = new();

        foreach (var item in ent.Comp.Storage.ContainedEntities)
        {
            if (!TryComp<PhysicalCompositionComponent>(item, out var physicalCompositionComponent)
                || !HasComp<MaterialComponent>(item))
                continue;

            var stack = 1;
            if (TryComp<StackComponent>(item, out var stackComponent))
                stack = stackComponent.Count;

            foreach (var (material, count) in physicalCompositionComponent.MaterialComposition)
            {
                if (!materials.TryAdd(material, count * stack))
                    materials[material] += count * stack;
            }
        }

        return materials;
    }

    private int CanSatisfyRecipe(Entity<FabricatorComponent> ent, FabricatorRecipePrototype recipe)
    {
        if (!ent.Comp.RecipeGroups.Contains(recipe.Group))
            return 0;

        var portions = ent.Comp.MaxPerBatch;

        var solids = GetSolids(ent);
        foreach (var (ingredient, count) in recipe.Solids)
        {
            if (!solids.TryGetValue(ingredient, out var solidStack))
                return 0;

            if (solidStack < count)
                return 0;

            portions = Math.Min(portions,
                solidStack / count);
        }

        var materials = GetMaterials(ent);
        foreach (var (ingredient, count) in recipe.Materials)
        {
            if (!materials.TryGetValue(ingredient, out var materialCount))
                return 0;

            if (materialCount < count)
                return 0;

            portions = Math.Min(portions,
                materialCount / count);
        }

        var reagents = GetReagents(ent);
        foreach (var ingredient in recipe.Reagents)
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

        if (ent.Comp.FabRunning)
            return;

        if (_item.GetSizePrototype(item.Size) > _item.GetSizePrototype(ent.Comp.MaxItemSize))
        {
            _popup.PopupClient(Loc.GetString("fabricator-component-item-too-big-warning", ("item", args.Used)), ent, args.User);
            return;
        }

        if (ent.Comp.Storage.Count >= ent.Comp.MaxItems)
        {
            _popup.PopupClient(Loc.GetString("fabricator-component-full-warning", ("fab", ent.Owner)), ent, args.User);
            return;
        }

        args.Handled = true;
        _hands.TryDropIntoContainer(args.User, args.Used, ent.Comp.Storage);

        UpdateUserInterfaceState(ent.Owner, ent.Comp);
        _audio.PlayPredicted(ent.Comp.AddItemSound, ent, args.User, AudioParams.Default.WithVolume(-2));

        Dirty(ent);
    }

    private void OnContentsChanged<TEvent>(Entity<FabricatorComponent> ent, ref TEvent args)
    {
       UpdateUserInterfaceState(ent.Owner, ent.Comp);
    }

    private void OnAnchorStateChanged(Entity<FabricatorComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            return;

        Stop(ent.Owner, ent.Comp);
    }

    private void OnPowerChanged(Entity<FabricatorComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        Stop(ent.Owner, ent.Comp);
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

        if (!_power.IsPowered(ent.Owner))
            return;

        foreach (var recipe in Recipes)
        {
            var portions = CanSatisfyRecipe(ent, recipe);
            if (portions <= 0)
                continue;

            ent.Comp.FabRunning = true;
            ent.Comp.SecondsRemaining = recipe.CompletionTime.Seconds * portions * ent.Comp.TimeMultiplier;
            ent.Comp.EndTime = _timing.CurTime.Add(TimeSpan.FromSeconds(ent.Comp.SecondsRemaining));
            ent.Comp.PortionedRecipe = (recipe, portions);

            _powerState.SetWorkingState(ent.Owner, true);

            SetAppearance(ent, FabricatorVisualState.Idle, false);
            SetAppearance(ent, FabricatorVisualState.Running, true);

            UpdateUserInterfaceState(ent.Owner, ent.Comp);
            _audio.PlayPredicted(ent.Comp.StartSound, ent, args.Actor, AudioParams.Default.WithVolume(-2));

            Dirty(ent);

            if (_net.IsServer)
            {
                ent.Comp.SoundStream =
                    _audio.PlayPvs(ent.Comp.LoopingSound, ent, AudioParams.Default.WithLoop(true).WithVolume(-2))
                        ?.Entity;
            }

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
        UpdateUserInterfaceState(ent.Owner, ent.Comp);
        _audio.PlayPredicted(ent.Comp.RemoveItemSound, ent, args.Actor, AudioParams.Default.WithVolume(-2));
        Dirty(ent);
    }
}
