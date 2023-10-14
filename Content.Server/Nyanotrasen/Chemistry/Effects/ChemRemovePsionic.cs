using Content.Shared.Chemistry.Reagent;
using Content.Server.Abilities.Psionics;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects
{
    /// <summary>
    /// Rerolls psionics once.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class ChemRemovePsionic : ReagentEffect
    {
        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-chem-remove-psionic", ("chance", Probability));

        public override void Effect(ReagentEffectArgs args)
        {
            if (args.Scale != 1f)
                return;

            var psySys = args.EntityManager.EntitySysManager.GetEntitySystem<PsionicAbilitiesSystem>();

            psySys.RemovePsionics(args.SolutionEntity);
        }
    }
}
