using Content.Shared._DV.Addictions;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects;

/// <summary>
/// A brief summary of the effect.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class SuppressAddictionEntityEffectSystem : EntityEffectSystem<AddictedComponent, SuppressAddition>
{
    [Dependency] private readonly SharedAddictionSystem _addiction = default!;
    protected override void Effect(Entity<AddictedComponent> entity, ref EntityEffectEvent<SuppressAddition> args)
    {
        var suppressionTime = args.Effect.Time * args.Scale;

        _addiction.TrySuppressAddiction(entity.Owner, suppressionTime);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class SuppressAddition : EntityEffectBase<SuppressAddition>
{
    /// <summary>
    ///     Amount of time that 1u suppresses addiction.
    /// </summary>
    [DataField]
    public float Time = 30f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addiction-suppression",
            ("chance", Probability));
}
