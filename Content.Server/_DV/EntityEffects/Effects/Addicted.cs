using Content.Shared._DV.Addictions;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class Addicted : EntityEffect
{
    /// <summary>
    /// How long should each metabolism cycle make the effect last for.
    /// </summary>
    [DataField]
    public float AddictionTime = 5f;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var addictionTime = AddictionTime;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            addictionTime *= reagentArgs.Scale.Float();
        }

        var addictionSystem = args.EntityManager.System<SharedAddictionSystem>();
        addictionSystem.TryApplyAddiction(args.TargetEntity, addictionTime);
    }
}
