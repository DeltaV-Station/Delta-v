using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Content.Server.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server._DV.EntityEffects.Effects;

/// <summary>
///     Changes glimmer when reaction happens.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class AffectsGlimmerEntityEffectSystem : EntityEffectSystem<MetaDataComponent, AffectsGlimmer>
{
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<AffectsGlimmer> args)
    {
        _glimmer.Glimmer += args.Effect.Amount;
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class AffectsGlimmer : EntityEffectBase<AffectsGlimmer>
{
    /// <summary>
    ///     Amount that is added or subtracted from glimmer.
    /// </summary>
    [DataField]
    public int Amount = 1;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-glimmer-reaction-effect", ("chance", Probability),
            ("amount", Amount));
}
