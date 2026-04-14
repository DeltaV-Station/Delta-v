using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects.Psionics;

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class RollPsionicAbility : EntityEffectBase<RollPsionicAbility>
{
    /// <summary>
    ///     Reroll multiplier.
    /// </summary>
    [DataField]
    public float BonusMultiplier = 1f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-chem-roll-psionic", ("multiplier", BonusMultiplier));
}
