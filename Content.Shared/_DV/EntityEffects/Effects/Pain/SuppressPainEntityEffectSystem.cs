using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.Effects.Pain;

// TODO: When Pain is moved to new Status, make this use StatusEffectsContainerComponent.
/// <summary>
/// Supresses pain based on how much of the pain suppressing reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class SuppressPainEntityEffectSystem : EntityEffectSystem<StatusEffectsComponent, SuppressPain>
{
    [Dependency] private readonly SharedPainSystem _pain = default!;
    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<SuppressPain> args)
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
