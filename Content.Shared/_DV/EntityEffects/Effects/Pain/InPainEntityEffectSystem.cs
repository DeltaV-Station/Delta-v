using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects.Pain;

// TODO: When Pain is moved to new Status, make this use StatusEffectsContainerComponent.
/// <summary>
/// Applies the pain status effect for an amount of time based on how much of the painful reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class InPainEntityEffectSystem : EntityEffectSystem<StatusEffectsComponent, InPain>
{
    [Dependency] private readonly SharedPainSystem _pain = default!;
    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<InPain> args)
    {
        var painTime = args.Effect.Time * args.Scale;

        _pain.TryApplyPain(entity, painTime);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class InPain : EntityEffectBase<InPain>
{
    /// <summary>
    /// How long should the addiction be per 1u of the reagent.
    /// </summary>
    [DataField]
    public float Time = 5f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted", ("chance", Probability));
}
