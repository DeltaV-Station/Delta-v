using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Effects;

/// <summary>
/// Effect that adds status effects to the player entity.
/// </summary>
public sealed partial class ApplyStatusEffect : BaseTraitEffect
{
    /// <summary>
    /// The status effects to add to the entity.
    /// </summary>
    [DataField(required: true)]
    public HashSet<EntProtoId> StatusEffects = new();

    public override void Apply(TraitEffectContext ctx)
    {
        foreach (var effect in StatusEffects)
        {
            ctx.StatusEffects.TrySetStatusEffectDuration(ctx.Player, effect);
        }
    }
}
