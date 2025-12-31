using Content.Server.Psionics;
using Content.Shared._DV.EntityEffects.Effects.Psionics;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Content.Server.Abilities.Psionics;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.EntityEffects.Effects.Psionics;

/// <summary>
///     Rerolls psionic abilities when at least 1u of the reagent is in the system.
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
