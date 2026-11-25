using Content.Server._DV.Psionics.Systems;
using Content.Shared._DV.EntityEffects.Effects.Psionics;
using Content.Shared.EntityEffects;
using Content.Shared._DV.Psionics.Components;

namespace Content.Server._DV.EntityEffects.Effects.Psionics;

/// <summary>
/// Rolls for a new psionic power.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class RollPsionicAbilityEntityEffectSystem : EntityEffectSystem<PotentialPsionicComponent, RollPsionicAbility>
{
    [Dependency] private readonly PsionicSystem _psionic = default!;
    protected override void Effect(Entity<PotentialPsionicComponent> psionic, ref EntityEffectEvent<RollPsionicAbility> args)
    {
        _psionic.TryRollPsionic(psionic, args.Effect.BonusMultiplier);
    }
}
