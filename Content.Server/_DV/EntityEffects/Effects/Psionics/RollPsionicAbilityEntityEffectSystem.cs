using Content.Server._DV.Psionics.Systems;
using Content.Shared._DV.EntityEffects.Effects.Psionics;
using Content.Shared.EntityEffects;
using Content.Shared._DV.Psionics.Components;

namespace Content.Server._DV.EntityEffects.Effects.Psionics;

/// <summary>
///     Rerolls psionic abilities when at least 1u of the reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class RollPsionicAbilityEntityEffectSystem : EntityEffectSystem<PotentialPsionicComponent, RollPsionicAbility>
{
    [Dependency] private readonly PsionicSystem _psionic = default!;
    protected override void Effect(Entity<PotentialPsionicComponent> entity, ref EntityEffectEvent<RollPsionicAbility> args)
    {
        _psionic.TryRollPsionic(entity, args.Effect.BonusMultiplier);
    }
}
