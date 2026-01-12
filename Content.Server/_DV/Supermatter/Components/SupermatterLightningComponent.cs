using System.Collections.Frozen;
using Content.Shared._Impstation.Supermatter.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
namespace Content.Server._DV.Supermatter.Components;

/// <summary>
/// This is used for...
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

    [DataField]
    public float LightningRangePowerScaling = 1000;

    [DataField]
    public float LightningRangeMin = 2;

    [DataField]
    public float LightningRangeMax = 7;

    [DataField]
    public EntProtoId LightningPrototype;

    [DataField]
    public bool EnableDamageThreshold = true;

    [DataField]
    public SortedDictionary<float, SupermatterLightningDamageThreshold> DamageThresholds;

    [DataField]
    public bool EnablePowerThresholds = true;

    [DataField]
    public SortedDictionary<float, SupermatterLightningPowerThreshold> PowerThresholds;
}

[DataDefinition] [Serializable] [NetSerializable]
public partial record struct SupermatterLightningPowerThreshold
{
    [DataField]
    public int Zaps;

    [DataField]
    public float? Chance;

    [DataField]
    public EntProtoId? LightningPrototype;
}

[DataDefinition] [Serializable] [NetSerializable]
public partial record struct SupermatterLightningDamageThreshold
{
    [DataField]
    public int Zaps;

    [DataField]
    public float? Chance;
};
