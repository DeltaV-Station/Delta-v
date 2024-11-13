using Content.Shared.DeltaV.Pain;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class PainSuppression : EntityEffect
{
    /// <summary>
    /// How long should the pain suppression last for each metabolism cycle
    /// </summary>
    [DataField]
    public float SuppressionTime = 30f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-pain-suppression", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var suppressionTime = SuppressionTime;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            suppressionTime *= reagentArgs.Scale.Float();
        }

        var painSystem = args.EntityManager.System<SharedPainSystem>();
        painSystem.TrySuppressPain(args.TargetEntity, suppressionTime);
    }
}
