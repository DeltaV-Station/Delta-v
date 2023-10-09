using Content.Shared.Chemistry.Reagent;
using Content.Server.Psionics;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects
{
    /// <summary>
    /// Rerolls psionics once.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class ChemRerollPsionic : ReagentEffect
    {
        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-chem-reroll-psionic", ("chance", Probability));

        /// <summary>
        /// Reroll multiplier.
        /// </summary>
        [DataField("bonusMultiplier")]
        public float BonusMuliplier = 1f;

        public override void Effect(ReagentEffectArgs args)
        {
            var psySys = args.EntityManager.EntitySysManager.GetEntitySystem<PsionicsSystem>();

            psySys.RerollPsionics(args.SolutionEntity, bonusMuliplier: BonusMuliplier);
        }
    }
}
