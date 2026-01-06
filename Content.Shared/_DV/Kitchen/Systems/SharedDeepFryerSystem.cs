using Content.Shared._DV.Kitchen.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Content.Shared.Verbs;
using Content.Shared.Hands.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Kitchen.Systems;

public abstract class SharedDeepFryerSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem Solution = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeepFryerComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DeepFryerComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<DeepFryerComponent, DeepFryerInsertDoAfterEvent>(OnInsertDoAfter);
        SubscribeLocalEvent<DeepFryerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
        SubscribeLocalEvent<DeepFryerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnComponentInit(Entity<DeepFryerComponent> ent, ref ComponentInit args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.ContainerName);
    }

    private void OnExamined(Entity<DeepFryerComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (Solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var solution) && solution.Volume <= 0)
            return;

        var qualityLevel = GetOilQualityLevel(ent.Comp.OilQuality);
        var (color, labelName) = GetOilQualityInfo(qualityLevel);

        args.PushMarkup(Loc.GetString("deep-fryer-oil-quality-examine",
            ("color", color.ToHex()),
            ("state", labelName)));
    }

    private void OnInteractUsing(Entity<DeepFryerComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Check if the item can be inserted
        if (!CanInsertItem(ent, args.Used, out var reason))
        {
            Popup.PopupClient(reason, ent, args.User);
            return;
        }

        args.Handled = true;

        // Start a do-after for inserting the item
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, 1f, new DeepFryerInsertDoAfterEvent(), ent, target: ent, used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BlockDuplicate = true,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnInsertDoAfter(Entity<DeepFryerComponent> ent, ref DeepFryerInsertDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Used == null)
            return;

        // Re-check if we can still insert (things might have changed)
        if (!CanInsertItem(ent, args.Used.Value, out var reason))
        {
            Popup.PopupClient(reason, ent, args.User);
            return;
        }

        // Insert the item
        if (TryInsertItem(ent, args.Used.Value, args.User))
        {
            args.Handled = true;
        }
    }

    private void OnGetAlternativeVerbs(Entity<DeepFryerComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
            return;

        var user = args.User;

        // Create an eject verb for each item in the basket
        foreach (var item in container.ContainedEntities)
        {
            var itemName = Name(item);
            var itemUid = item;

            var verb = new AlternativeVerb
            {
                Text = Loc.GetString("deep-fryer-eject-item", ("item", itemName)),
                Category = VerbCategory.Eject,
                Act = () => TryEjectItem(ent, itemUid, user),
                Priority = 1
            };

            args.Verbs.Add(verb);
        }
    }

    /// <summary>
    /// Attempts to eject an item from the deep fryer
    /// </summary>
    [PublicAPI]
    public bool TryEjectItem(Entity<DeepFryerComponent> ent, EntityUid item, EntityUid user)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
            return false;

        if (!container.Contains(item))
            return false;

        // Remove from container
        if (!_container.Remove(item, container))
            return false;

        // Try to put in user's hands, otherwise drop at fryer location
        if (!_hands.TryPickupAnyHand(user, item))
        {
            var xform = Transform(ent);
            Xform.SetCoordinates(item, xform.Coordinates);
        }

        Popup.PopupClient(Loc.GetString("deep-fryer-eject-item-success", ("item", item)), ent, user);
        return true;
    }

    /// <summary>
    /// Checks if an item can be inserted into the deep fryer
    /// </summary>
    [PublicAPI]
    public bool CanInsertItem(Entity<DeepFryerComponent> ent, EntityUid item, out string reason)
    {
        reason = string.Empty;

        // Skip the popup entirely if we're transferring solutions
        if (HasComp<SolutionTransferComponent>(item))
            return false;

        // Check blacklist first since it should override
        if (_whitelist.IsBlacklistPass(ent.Comp.Blacklist, item))
        {
            reason = Loc.GetString("deep-fryer-blacklist-item");
            return false;
        }

        // Check whitelist
        if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, item))
        {
            reason = Loc.GetString("deep-fryer-not-food");
            return false;
        }

        // Get the container
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
        {
            reason = Loc.GetString("deep-fryer-no-container");
            return false;
        }

        // Check if the fryer is full
        if (container.ContainedEntities.Count >= ent.Comp.MaxItems)
        {
            reason = Loc.GetString("deep-fryer-full");
            return false;
        }

        // Check if there's enough oil
        if (!HasEnoughOil(ent))
        {
            reason = Loc.GetString("deep-fryer-insufficient-oil");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to insert an item into the deep fryer
    /// </summary>
    [PublicAPI]
    public bool TryInsertItem(Entity<DeepFryerComponent> ent, EntityUid item, EntityUid? user)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
            return false;

        // Insert the item
        if (!_container.Insert(item, container))
            return false;

        Popup.PopupClient(Loc.GetString("deep-fryer-insert-item", ("item", item)), ent, user ?? EntityUid.Invalid);
        return true;
    }

    /// <summary>
    /// Checks if the fryer has enough oil to fry items
    /// </summary>
    protected bool HasEnoughOil(Entity<DeepFryerComponent> ent)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var solution))
            return false;

        // Check if there's enough total volume
        if (solution.Volume < ent.Comp.MinimumOilVolume)
            return false;

        // Check if any of the reagents in the solution are valid frying oils
        foreach (var reagent in solution.Contents)
        {
            if (ent.Comp.FryingOils.Contains(reagent.Reagent.Prototype))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the number of items currently in the fryer
    /// </summary>
    [PublicAPI]
    public int GetItemCount(Entity<DeepFryerComponent> ent)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
            return 0;

        return container.ContainedEntities.Count;
    }

    /// <summary>
    /// Determines the oil quality level from the quality value
    /// </summary>
    [PublicAPI]
    public static OilQuality GetOilQualityLevel(float quality)
    {
        return quality switch
        {
            >= 0.9f => OilQuality.Pristine,
            >= 0.7f => OilQuality.Clean,
            >= 0.5f => OilQuality.Used,
            >= 0.3f => OilQuality.Dirty,
            _ => OilQuality.Foul
        };
    }

    /// <summary>
    /// Calculates the oil degradation multiplier based on current oil volume
    /// Less oil = faster degradation
    /// </summary>
    protected float CalculateOilDegradationMultiplier(Entity<DeepFryerComponent> ent)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var solution))
            return 1.0f;

        var maxVolume = solution.MaxVolume;
        var currentVolume = solution.Volume;
        var minVolume = ent.Comp.MinimumOilVolume;

        // If we don't have enough oil range, just return 1.0
        if (maxVolume <= minVolume)
            return 1.0f;

        // Linear interpolation between 1.0x (at max volume) and MinOilVolumeDegradationMultiplier (at min volume)
        // 1.0 + (maxVolume - currentVolume) / (maxVolume - minVolume) * (multiplier - 1.0)
        var volumeRatio = (float)((maxVolume - currentVolume) / (maxVolume - minVolume));
        var multiplier = 1.0f + volumeRatio * (ent.Comp.MinOilVolumeDegradationMultiplier - 1.0f);

        return Math.Max(1.0f, multiplier);
    }

    /// <summary>
    /// Gets the color and label name for a given oil quality level
    /// </summary>
    [PublicAPI]
    public (Color color, string labelName) GetOilQualityInfo(OilQuality quality)
    {
        return quality switch
        {
            OilQuality.Pristine => (Color.Green, Loc.GetString("deep-fryer-oil-quality-pristine")),
            OilQuality.Clean => (Color.White, Loc.GetString("deep-fryer-oil-quality-clean")),
            OilQuality.Used => (Color.Yellow, Loc.GetString("deep-fryer-oil-quality-used")),
            OilQuality.Dirty => (Color.Orange, Loc.GetString("deep-fryer-oil-quality-dirty")),
            OilQuality.Foul => (Color.Red, Loc.GetString("deep-fryer-oil-quality-foul")),
            _ => (Color.White, Loc.GetString("deep-fryer-oil-quality-unknown"))
        };
    }
}

[Serializable, NetSerializable]
public sealed partial class DeepFryerInsertDoAfterEvent : SimpleDoAfterEvent;
