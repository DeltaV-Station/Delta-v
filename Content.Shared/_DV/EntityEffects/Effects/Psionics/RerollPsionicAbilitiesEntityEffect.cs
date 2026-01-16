using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects.Psionics;

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
