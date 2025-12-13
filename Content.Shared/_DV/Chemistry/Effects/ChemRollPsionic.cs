using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Chemistry.Effects;

/// <summary>
/// Rolls psionics once.
/// </summary>
[UsedImplicitly]
public sealed partial class ChemRollPsionic : EventEntityEffect<ChemRollPsionic>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-chem-roll-psionic", ("multiplier", BonusMultiplier));

    /// <summary>
    /// Chance Multiplier.
    /// </summary>
    [DataField]
    public float BonusMultiplier = 1f;
}
