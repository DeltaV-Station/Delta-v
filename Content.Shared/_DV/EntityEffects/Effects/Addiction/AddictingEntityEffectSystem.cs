using Content.Shared._DV.Addictions;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects.Addiction;

// TODO: When Addiction is moved to new Status, make this use StatusEffectsContainerComponent.
/// <summary>
/// Applies the addition status effect for an amount of time based on how much of the addicting reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class AddictingEntityEffectSystem : EntityEffectSystem<StatusEffectsComponent, Addicting>
{
    [Dependency] private readonly SharedAddictionSystem _addiction = default!;
    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<Addicting> args)
    {
        var addictionTime = args.Effect.Time * args.Scale;

        _addiction.TryApplyAddiction(entity, addictionTime);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class Addicting : EntityEffectBase<Addicting>
{
    /// <summary>
    /// How long should the pain be per 1u of the reagent.
    /// </summary>
    [DataField]
    public float Time = 5f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted", ("chance", Probability));
}
