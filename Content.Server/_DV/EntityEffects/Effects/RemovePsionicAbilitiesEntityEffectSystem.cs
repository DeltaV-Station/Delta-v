using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Content.Server.Abilities.Psionics;
using Content.Server.Psionics;

namespace Content.Server._DV.EntityEffects.Effects;

/// <summary>
///     Removes psionic abilities when at least 1u of the reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class RemovePsionicAbilitiesEntityEffectSystem : EntityEffectSystem<PotentialPsionicComponent, RemovePsionicAbilities>
{
    [Dependency] private readonly PsionicsSystem _psionic = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilities = default!;
    protected override void Effect(Entity<PotentialPsionicComponent> entity, ref EntityEffectEvent<RemovePsionicAbilities> args)
    {
        if (args.Scale != 1f)
            return;

        _psionicAbilities.RemovePsionics(entity);
        _psionic.GrantNewPsionicReroll(entity);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class RemovePsionicAbilities : EntityEffectBase<RemovePsionicAbilities>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-chem-remove-psionic", ("chance", Probability));
}
