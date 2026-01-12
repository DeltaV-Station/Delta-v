using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Tips.Conditions;

/// <summary>
/// Base class for tip conditions. Implementations check if a tip should be shown to a player.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class TipCondition
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
    public bool Evaluate(TipConditionContext ctx)
    {
        var result = EvaluateImplementation(ctx);
        return result ^ Invert;
    }

    protected abstract bool EvaluateImplementation(TipConditionContext ctx);
}

/// <summary>
/// Context passed to tip conditions for evaluation.
/// Contains references to the player and relevant systems.
/// </summary>
public sealed class TipConditionContext
{
    public required EntityUid Player { get; init; }
    public required ICommonSession Session { get; init; }
    public required IEntityManager EntMan { get; init; }
    public required IPrototypeManager Proto { get; init; }
    public required IComponentFactory CompFactory { get; init; }
    public required ILogManager LogMan { get; init; }
}
