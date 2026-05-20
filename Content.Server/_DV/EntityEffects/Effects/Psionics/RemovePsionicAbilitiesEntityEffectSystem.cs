using Content.Server._DV.Psionics.Systems;
using Content.Shared._DV.EntityEffects.Effects.Psionics;
using Content.Shared.EntityEffects;
using Content.Shared._DV.Psionics.Components;

namespace Content.Server._DV.EntityEffects.Effects.Psionics;

/// <summary>
/// Removes psionic abilities.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class RemovePsionicAbilitiesEntityEffectSystem : EntityEffectSystem<PsionicComponent, RemovePsionicAbilities>
{
    [Dependency] private readonly PsionicSystem _psionicSystem = default!;

    protected override void Effect(Entity<PsionicComponent> psionic, ref EntityEffectEvent<RemovePsionicAbilities> args)
    {
        _psionicSystem.MindBreakEntity(psionic.Owner);
    }
}
