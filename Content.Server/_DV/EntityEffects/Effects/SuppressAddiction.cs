using Content.Shared._DV.Addictions;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class SuppressAddiction : EntityEffect
{
    /// <summary>
    /// How long should the addiction suppression last for each metabolism cycle
    /// </summary>
    [DataField]
    public float SuppressionTime = 30f;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addiction-suppression",
            ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var suppressionTime = SuppressionTime;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            suppressionTime *= reagentArgs.Scale.Float();
        }

        var addictionSystem = args.EntityManager.System<SharedAddictionSystem>();
        addictionSystem.TrySuppressAddiction(args.TargetEntity, suppressionTime);
    }
}
