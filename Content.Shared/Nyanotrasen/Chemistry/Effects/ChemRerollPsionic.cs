using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nyanotrasen.Chemistry.Effects
{
    /// <summary>
    /// Rerolls psionics once.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class ChemRerollPsionic : EventEntityEffect<ChemRerollPsionic>
    {
        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-chem-reroll-psionic", ("chance", Probability));

        /// <summary>
        /// Reroll multiplier.
        /// </summary>
        [DataField("bonusMultiplier")]
        public float BonusMuliplier = 1f;
    }
}
