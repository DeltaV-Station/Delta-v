using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Content.Server.Abilities.Psionics;
using Content.Server.Psionics;

namespace Content.Server._DV.EntityEffects.Effects;

// TODO: When Pain is moved to new Status, make this use StatusEffectsContainerComponent.
/// <summary>
/// Supresses pain based on how much of the pain suppressing reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class RerollPsionicAbilitiesEntityEffectSystem : EntityEffectSystem<PotentialPsionicComponent, RerollPsionicAbilities>
{
    [Dependency] private readonly PsionicsSystem _psionic = default!;
    protected override void Effect(Entity<PotentialPsionicComponent> entity, ref EntityEffectEvent<RerollPsionicAbilities> args)
    {
        if (args.Scale != 1f)
            return;

        _psionic.RerollPsionics(entity, bonusMuliplier: args.Effect.BonusMultiplier);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class RerollPsionicAbilities : EntityEffectBase<RerollPsionicAbilities>
{
    /// <summary>
    ///     Reroll multiplier.
    /// </summary>
    [DataField("bonusMultiplier")]
    public float BonusMultiplier = 1f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-chem-reroll-psionic", ("chance", Probability));
}
