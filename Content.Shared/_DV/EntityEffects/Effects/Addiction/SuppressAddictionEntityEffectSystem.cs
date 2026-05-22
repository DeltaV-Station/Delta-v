using Content.Shared._DV.Addictions;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects.Addiction;

// TODO: When Addiction is moved to new Status, make this use StatusEffectsContainerComponent.
/// <summary>
/// Supresses addition for an amount of time based on how much of the suppressive reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class SuppressAddictionEntityEffectSystem : EntityEffectSystem<StatusEffectsComponent, SuppressAddiction>
{
    [Dependency] private readonly SharedAddictionSystem _addiction = default!;
    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<SuppressAddiction> args)
    {
        var suppressionTime = args.Effect.Time * args.Scale;

        _addiction.TrySuppressAddiction(entity.Owner, suppressionTime);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class SuppressAddiction : EntityEffectBase<SuppressAddiction>
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
