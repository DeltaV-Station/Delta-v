using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects;

/// <summary>
/// A brief summary of the effect.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class SuppressPainEntityEffectSystem : EntityEffectSystem<PainComponent, SuppressPain>
{
    [Dependency] private readonly SharedPainSystem _pain = default!;
    protected override void Effect(Entity<PainComponent> entity, ref EntityEffectEvent<SuppressPain> args)
    {
        var suppressionTime = args.Effect.Time * args.Scale;

        _pain.TrySuppressPain(entity, suppressionTime, args.Effect.Level);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class SuppressPain : EntityEffectBase<SuppressPain>
{
    /// <summary>
    /// How long should the pain suppression last for each metabolism cycle
    /// </summary>
    [DataField]
    public float Time = 30f;

    /// <summary>
    /// The strength level of the pain suppression.
    /// </summary>
    [DataField]
    public PainSuppressionLevel Level = PainSuppressionLevel.Normal;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-pain-suppression",
            ("chance", Probability),
            ("level", Level.ToString().ToLowerInvariant()));
}
