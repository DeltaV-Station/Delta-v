using Content.Shared._DV.CCVars;
using Content.Shared._DV.Traits;
using Content.Shared._DV.Traits.Conditions;
using Content.Shared._DV.Traits.Effects;
using Content.Shared.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Traits;

/// <summary>
/// Server system that validates and applies traits to players on spawn.
/// </summary>
public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private int _maxTraitCount;
    private int _maxTraitPoints;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        Subs.CVar(_config, DCCVars.MaxTraitCount, value => _maxTraitCount = value, true);
        Subs.CVar(_config, DCCVars.MaxTraitPoints, value => _maxTraitPoints = value, true);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Check if player's job allows traits
        if (args.JobId == null ||
            !_prototype.TryIndex<JobPrototype>(args.JobId, out var jobProto) ||
            !jobProto.ApplyTraits)
            return;

        // Get species ID for condition checking
        string? speciesId = null;
        if (TryComp<HumanoidAppearanceComponent>(args.Mob, out var humanoid))
            speciesId = humanoid.Species;

        // Track disabled traits and reasons
        var disabledTraits = new Dictionary<ProtoId<TraitPrototype>, List<string>>();

        // Validate and collect valid traits
        var validTraits = ValidateTraits(args.Mob, args.Profile.TraitPreferences, args.Player, args.JobId, speciesId, disabledTraits);

        // Apply valid traits
        foreach (var traitId in validTraits)
        {
            if (!_prototype.TryIndex(traitId, out var trait))
                continue;

            ApplyTrait(args.Mob, trait);
        }

        // Send disabled traits notification to client if any were rejected
        if (disabledTraits.Count > 0)
        {
            RaiseNetworkEvent(new DisabledTraitsEvent(disabledTraits), args.Player);
        }
    }

    /// <summary>
    /// Validates a set of trait selections against all rules and returns the valid subset.
    /// </summary>
    private HashSet<ProtoId<TraitPrototype>> ValidateTraits(
        EntityUid player,
        IReadOnlySet<ProtoId<TraitPrototype>> selectedTraits,
        ICommonSession? session,
        string? jobId,
        string? speciesId,
        Dictionary<ProtoId<TraitPrototype>, List<string>> disabledTraits)
    {
        var validTraits = new HashSet<ProtoId<TraitPrototype>>();
        var totalPoints = 0;
        var traitCount = 0;
        var categoryTraitCounts = new Dictionary<ProtoId<TraitCategoryPrototype>, int>();
        var categoryPointTotals = new Dictionary<ProtoId<TraitCategoryPrototype>, int>();

        // Build condition context
        var conditionCtx = new TraitConditionContext
        {
            Player = player,
            Session = session,
            EntMan = EntityManager,
            Proto = _prototype,
            CompFactory = _factory,
            LogMan = _log,
            JobId = jobId,
            SpeciesId = speciesId,
        };

        foreach (var traitId in selectedTraits)
        {
            if (!_prototype.TryIndex(traitId, out var trait))
            {
                Log.Warning($"Unknown trait ID in player preferences: {traitId}");
                continue;
            }

            var rejectionReasons = new List<string>();

            // Check global trait count limit
            if (traitCount >= _maxTraitCount)
            {
                Log.Warning($"Trait {traitId} rejected: global trait count limit ({_maxTraitCount}) exceeded");
                rejectionReasons.Add(Loc.GetString("disabled-traits-reason-global-limit"));
                disabledTraits[traitId] = rejectionReasons;
                continue;
            }

            // Check global points limit
            if (totalPoints + trait.Cost > _maxTraitPoints)
            {
                Log.Warning($"Trait {traitId} rejected: global points limit ({_maxTraitPoints}) would be exceeded");
                rejectionReasons.Add(Loc.GetString("disabled-traits-reason-points-limit"));
                disabledTraits[traitId] = rejectionReasons;
                continue;
            }

            // Check category limits
            if (!ValidateCategoryLimits(trait, categoryTraitCounts, categoryPointTotals, rejectionReasons))
            {
                Log.Warning($"Trait {traitId} rejected: category limits exceeded");
                disabledTraits[traitId] = rejectionReasons;
                continue;
            }

            // Check conflicts with already selected traits
            var hasConflict = false;
            foreach (var validTraitId in validTraits)
            {
                // Check if current trait conflicts with valid trait
                if (trait.Conflicts.Contains(validTraitId))
                {
                    Log.Warning($"Trait {traitId} rejected: conflicts with {validTraitId}");
                    if (_prototype.TryIndex(validTraitId, out var conflictTrait))
                    {
                        rejectionReasons.Add(Loc.GetString("disabled-traits-reason-conflict",
                            ("trait", Loc.GetString(conflictTrait.Name))));
                    }
                    hasConflict = true;
                    break;
                }

                // Check if valid trait conflicts with current trait
                if (_prototype.TryIndex(validTraitId, out var validTrait) &&
                    validTrait.Conflicts.Contains(traitId))
                {
                    Log.Warning($"Trait {traitId} rejected: {validTraitId} conflicts with it");
                    rejectionReasons.Add(Loc.GetString("disabled-traits-reason-conflict",
                        ("trait", Loc.GetString(validTrait.Name))));
                    hasConflict = true;
                    break;
                }
            }

            if (hasConflict)
            {
                disabledTraits[traitId] = rejectionReasons;
                continue;
            }

            // Check all conditions
            if (!CheckConditions(trait, conditionCtx, rejectionReasons))
            {
                Log.Warning($"Trait {traitId} rejected: conditions not met");
                disabledTraits[traitId] = rejectionReasons;
                continue;
            }

            // Trait is valid, add it
            validTraits.Add(traitId);
            totalPoints += trait.Cost;
            traitCount++;

            // Update category tracking
            categoryTraitCounts.TryGetValue(trait.Category, out var catCount);
            categoryTraitCounts[trait.Category] = catCount + 1;

            categoryPointTotals.TryGetValue(trait.Category, out var catPoints);
            categoryPointTotals[trait.Category] = catPoints + trait.Cost;
        }

        return validTraits;
    }

    /// <summary>
    /// Validates that adding a trait wouldn't exceed category-specific limits.
    /// </summary>
    private bool ValidateCategoryLimits(
        TraitPrototype trait,
        Dictionary<ProtoId<TraitCategoryPrototype>, int> categoryTraitCounts,
        Dictionary<ProtoId<TraitCategoryPrototype>, int> categoryPointTotals,
        List<string> rejectionReasons)
    {
        if (!_prototype.TryIndex(trait.Category, out var category))
            return true; // Unknown category, allow it

        categoryTraitCounts.TryGetValue(trait.Category, out var currentCount);
        categoryPointTotals.TryGetValue(trait.Category, out var currentPoints);

        // Check category trait count limit
        if (category.MaxTraits.HasValue && currentCount >= category.MaxTraits.Value)
        {
            rejectionReasons.Add(Loc.GetString("disabled-traits-reason-category-limit",
                ("category", Loc.GetString(category.Name))));
            return false;
        }

        // Check category points limit
        if (category.MaxPoints.HasValue && currentPoints + trait.Cost > category.MaxPoints.Value)
        {
            rejectionReasons.Add(Loc.GetString("disabled-traits-reason-category-points",
                ("category", Loc.GetString(category.Name))));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks all conditions on a trait.
    /// </summary>
    private bool CheckConditions(TraitPrototype trait, TraitConditionContext ctx, List<string> rejectionReasons)
    {
        foreach (var condition in trait.Conditions)
        {
            if (condition.Evaluate(ctx))
                continue;

            // Get human-readable reason from the condition
            var tooltip = condition.GetTooltip(ctx.Proto, Loc);

            if (!string.IsNullOrEmpty(tooltip))
                rejectionReasons.Add(tooltip);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Applies a trait's effects to an entity.
    /// </summary>
    private void ApplyTrait(EntityUid player, TraitPrototype trait)
    {
        var transform = Transform(player);

        var effectCtx = new TraitEffectContext
        {
            Player = player,
            EntMan = EntityManager,
            Proto = _prototype,
            CompFactory = _factory,
            LogMan = _log,
            Transform = transform,
        };

        foreach (var effect in trait.Effects)
        {
            try
            {
                // Handle SpawnItemInHandEffect specially since it needs server-side systems
                if (effect is SpawnItemInHandEffect spawnEffect)
                    ApplySpawnItemEffect(player, spawnEffect, transform);
                else
                    effect.Apply(effectCtx);
            }
            catch (Exception e)
            {
                Log.Error($"Error applying effect {effect.GetType().Name} for trait {trait.ID}: {e}");
            }
        }
    }

    /// <summary>
    /// Handles the SpawnItemInHandEffect since it requires server-side systems.
    /// </summary>
    private void ApplySpawnItemEffect(EntityUid player, SpawnItemInHandEffect effect, TransformComponent transform)
    {
        if (!TryComp<HandsComponent>(player, out var hands))
        {
            Log.Warning("Cannot spawn trait item: player has no hands component");
            return;
        }

        var coords = transform.Coordinates;
        var item = Spawn(effect.Item, coords);

        if (!_hands.TryPickup(player, item, checkActionBlocker: false, handsComp: hands))
            Log.Debug($"Could not pick up trait item {effect.Item}, leaving at feet");
    }
}
