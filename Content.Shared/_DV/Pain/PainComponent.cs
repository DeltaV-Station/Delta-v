using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Pain;

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
    /// The current level of pain suppression
    /// </summary>
    [DataField]
    public PainSuppressionLevel CurrentSuppressionLevel = PainSuppressionLevel.Normal;

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

    /// <summary>
    /// When to show the next pain effect popup
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextPopupTime;

    /// <summary>
    /// The dataset of pain effect messages to display
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> DatasetPrototype = "PainEffects";

    /// <summary>
    /// Minimum time between pain popups in seconds
    /// </summary>
    [DataField]
    public float MinimumPopupDelay = 1f;

    /// <summary>
    /// Maximum time between pain popups in seconds
    /// </summary>
    [DataField]
    public float MaximumPopupDelay = 40f;
}
