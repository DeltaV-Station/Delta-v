using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.AiDetector;

/// <summary>
/// Changes an appearance data string depending on distance to any <c>AiDetectable</c> entities.
/// </summary>
[RegisterComponent, Access(typeof(AiDetectorSystem))]
public sealed partial class AiDetectorComponent : Component
{
    /// <summary>
    /// The string to use for appearance data when there is no AI nearby.
    /// </summary>
    [DataField]
    public string Default = "none";

    /// <summary>
    /// Each range and state to use.
    /// The first one found is used, so have the shortest range first.
    /// </summary>
    [DataField(required: true)]
    public List<AiDetectorRange> Ranges = new();

    /// <summary>
    /// The state currently shown.
    /// </summary>
    [DataField]
    public string State = string.Empty;

    /// <summary>
    /// How long to wait between updates.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5);

    /// <summary>
    /// When to next update state.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}

[DataRecord]
public partial record struct AiDetectorRange(string State = "", float Range = 0f);
