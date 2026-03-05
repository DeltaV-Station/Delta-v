using Content.Shared._DV.EntityEffects.Effects.Psionics;
using Content.Shared.EntityEffects;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;

namespace Content.Server._DV.EntityEffects.Effects.Psionics;

/// <summary>
/// Removes psionic abilities.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class RemovePsionicAbilitiesEntityEffectSystem : EntityEffectSystem<PotentialPsionicComponent, RemovePsionicAbilities>
{
    protected override void Effect(Entity<PotentialPsionicComponent> entity, ref EntityEffectEvent<RemovePsionicAbilities> args)
    {
        var ev = new PsionicMindBrokenEvent(true);
        RaiseLocalEvent(entity, ref ev);
    }
}
