using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.DeltaV.Pain;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class PainComponent : Component
{
    /// <summary>
    /// Whether pain effects are currently suppressed by painkillers
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Suppressed;

    /// <summary>
    /// The last time painkillers were administered
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan LastPainkillerTime;

    /// <summary>
    /// When the pain suppression effect ends
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan SuppressionEndTime;

    /// <summary>
    /// When to next update this component
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdateTime;
}
