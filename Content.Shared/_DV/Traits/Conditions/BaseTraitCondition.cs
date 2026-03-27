using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Base class for trait conditions. Implementations check if a trait can be applied to a player.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class BaseTraitCondition
{
    /// <summary>
    /// If true, inverts the result of the condition.
    /// </summary>
    [DataField]
    public bool Invert;

    /// <summary>
    /// Evaluates the condition, applying inversion if configured.
    /// </summary>
    [PublicAPI]
    public bool Evaluate(TraitConditionContext ctx)
    {
        var result = EvaluateImplementation(ctx);
        return result ^ Invert;
    }

    /// <summary>
    /// Generates a human-readable tooltip describing this condition's requirements.
    /// </summary>
    [PublicAPI]
    public abstract string GetTooltip(IPrototypeManager proto, ILocalizationManager loc, int depth);

    protected abstract bool EvaluateImplementation(TraitConditionContext ctx);
}

/// <summary>
/// Context passed to trait conditions for evaluation.
/// Contains references to the player and relevant systems.
/// </summary>
public sealed class TraitConditionContext
{
    public required EntityUid Player { get; init; }
    public required ICommonSession? Session { get; init; }
    public required IEntityManager EntMan { get; init; }
    public required IPrototypeManager Proto { get; init; }
    public required IComponentFactory CompFactory { get; init; }
    public required ILogManager LogMan { get; init; }

    /// <summary>
    /// The job ID of the player, if available.
    /// </summary>
    public ProtoId<JobPrototype>? JobId { get; init; }

    /// <summary>
    /// The species ID of the player, if available.
    /// </summary>
    public ProtoId<SpeciesPrototype>? SpeciesId { get; init; }

    /// <summary>
    /// The <see cref="HumanoidCharacterProfile"/> of the player, if available.
    /// </summary>
    public HumanoidCharacterProfile? Profile { get; init; }
}
