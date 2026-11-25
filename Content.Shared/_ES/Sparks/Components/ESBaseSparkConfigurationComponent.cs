using Robust.Shared.Prototypes;

namespace Content.Shared._ES.Sparks.Components;

/// <summary>
/// This is a base component that contains details about
/// configuring sparks in order to be reused with minimal duplication
/// </summary>
public abstract partial class ESBaseSparkConfigurationComponent : Component
{
    /// <summary>
    /// Number of sparks
    /// </summary>
    [DataField]
    public int Count = 3;

    /// <summary>
    /// Chance for sparks to occur
    /// </summary>
    [DataField]
    public float Prob = 1f;

    /// <summary>
    /// Chance a successful spark hit will also spawn a tile fire
    /// </summary>
    [DataField]
    public float TileFireChance;

    /// <summary>
    /// Spark prototypes
    /// </summary>
    [DataField]
    public EntProtoId SparkPrototype = ESSparksSystem.DefaultSparks;
}
