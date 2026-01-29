using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
namespace Content.Server._DV.Supermatter.Components;

/// <summary>
/// This is used for the lightning side effect of the supermatter.
/// </summary>
[RegisterComponent][AutoGenerateComponentPause]
public sealed partial class SupermatterLightningComponent : Component
{
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// The chance for supermatter lightning to strike random coordinates instead of an entity
    /// </summary>
    [DataField]
    public float ZapHitCoordinatesChance = 0.75f;

    /// <summary>
    /// The interval at which the supermatter will shoot lightning.
    /// </summary>
    [DataField]
    public TimeSpan ZapInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The variance in the interval at which the supermatter will shoot lightning.
    /// </summary>
    [DataField]
    public float ZapIntervalVariance = 2f;

    /// <summary>
    /// The next time the supermatter will shoot lightning.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))][AutoPausedField]
    public TimeSpan? NextZapTime;

    /// <summary>
    /// The divisor applied to the supermatter's Power to determine the range of lightning strikes. This is clamped to be between <see cref="LightningRangeMin"/> and <see cref="LightningRangeMax"/>.
    /// </summary>
    [DataField]
    public float LightningRangePowerScaling = 1000;

    /// <summary>
    /// The minimum range of lightning strikes to be applied after scaling Power.
    /// </summary>
    [DataField]
    public float LightningRangeMin = 2;

    /// <summary>
    /// The maximum range of lightning strikes to be applied after scaling Power.
    /// </summary>
    [DataField]
    public float LightningRangeMax = 7;

    /// <summary>
    /// The prototype of the lightning entity to spawn.
    /// </summary>
    [DataField]
    public EntProtoId LightningPrototype;

    /// <summary>
    /// Whether to use the damage thresholds for calculating lightning strikes.
    /// </summary>
    [DataField]
    public bool EnableDamageThreshold = true;

    /// <summary>
    /// The damage thresholds for lightning strikes.
    /// </summary>
    [DataField]
    public SortedDictionary<float, SupermatterLightningDamageThreshold> DamageThresholds;

    /// <summary>
    /// Whether to use the power thresholds for calculating lightning strikes.
    /// </summary>
    [DataField]
    public bool EnablePowerThresholds = true;

    /// <summary>
    /// The power thresholds for lightning strikes.
    /// </summary>
    [DataField]
    public SortedDictionary<float, SupermatterLightningPowerThreshold> PowerThresholds;
}

[DataDefinition] [Serializable] [NetSerializable]
public partial record struct SupermatterLightningPowerThreshold
{
    /// <summary>
    /// The number of zaps to add if this threshold is met.
    /// </summary>
    [DataField]
    public int Zaps;

    /// <summary>
    /// The chance that the zaps will be added.
    /// </summary>
    [DataField]
    public float? Chance;

    /// <summary>
    /// The prototype of the lightning entity to spawn, or null if it should not be changed.
    /// </summary>
    [DataField]
    public EntProtoId? LightningPrototype;
}

[DataDefinition] [Serializable] [NetSerializable]
public partial record struct SupermatterLightningDamageThreshold
{
    /// <summary>
    /// The number of zaps to add if this threshold is met.
    /// </summary>
    [DataField]
    public int Zaps;

    /// <summary>
    /// The chance that the zaps will be added.
    /// </summary>
    [DataField]
    public float? Chance;
};
