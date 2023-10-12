using Content.Shared.Chemistry.Reagent;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReactionEffects;

[DataDefinition]
public sealed partial class ChangeGlimmerReactionEffect : ReagentEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-glimmer-reaction-effect", ("chance", Probability),
            ("count", Count));

    /// <summary>
    ///     Added to glimmer when reaction occurs.
    /// </summary>
    [DataField("count")]
    public int Count = 1;

    public override void Effect(ReagentEffectArgs args)
    {
        var glimmersys = args.EntityManager.EntitySysManager.GetEntitySystem<GlimmerSystem>();

        glimmersys.Glimmer += Count;
    }
}
