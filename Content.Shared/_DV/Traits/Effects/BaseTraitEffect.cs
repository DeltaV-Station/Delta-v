using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Effects;

/// <summary>
/// Base class for trait effects. Implementations apply modifications to an entity when a trait is selected.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class BaseTraitEffect
{
    /// <summary>
    /// Applies the effect to the target entity.
    /// </summary>
    [PublicAPI]
    public abstract void Apply(TraitEffectContext ctx);
}

/// <summary>
/// Context passed to trait effects for application.
/// Contains references to the player entity and relevant systems.
/// </summary>
public sealed class TraitEffectContext
{
    public required EntityUid Player { get; init; }
    public required IEntityManager EntMan { get; init; }
    public required IPrototypeManager Proto { get; init; }
    public required IComponentFactory CompFactory { get; init; }
    public required ILogManager LogMan { get; init; }
    public required TransformComponent Transform { get; init; }
}
