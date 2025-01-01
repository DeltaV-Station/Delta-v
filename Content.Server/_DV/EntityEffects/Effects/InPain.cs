using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class InPain : EntityEffect
{
    /// <summary>
    /// How long should each metabolism cycle make the effect last for.
    /// </summary>
    [DataField]
    public float PainTime = 5f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var painTime = PainTime;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            painTime *= reagentArgs.Scale.Float();
        }

        var painSystem = args.EntityManager.System<SharedPainSystem>();
        painSystem.TryApplyPain(args.TargetEntity, painTime);
    }
}
