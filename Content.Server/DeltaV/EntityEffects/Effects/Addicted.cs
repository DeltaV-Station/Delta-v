using Content.Shared.DeltaV.Addictions;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class Addicted : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var addictionSystem = args.EntityManager.EntitySysManager.GetEntitySystem<SharedAddictionSystem>();
        addictionSystem.TryApplyAddiction(args.TargetEntity);
    }
}
