using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.ChronicPain.Components;

/// <summary>
/// This component is used to mark people who have the chronic pain trait.
///
/// If someone is currently the chronic pain status effect // TODO
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ChronicPainComponent : Component
{
    /// <summary>
    /// If pain is trying to be suppressed and no suppression time is given, it should default to this time.
    /// </summary>
    [DataField]
    public TimeSpan DefaultSuppressionTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The default suppression time on map init, so people don't have to eat a pill right away.
    /// </summary>
    [DataField]
    public TimeSpan DefaultSuppressionTimeOnInit = TimeSpan.FromMinutes(5);

    /// <summary>
    /// When to next update this component
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    [AutoNetworkedField]
    public TimeSpan NextUpdateTime;

    /// <summary>
    /// When to show the next pain effect popup.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    [AutoNetworkedField]
    public TimeSpan NextPopupTime;

    /// <summary>
    /// The dataset of pain effect messages to display.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> DatasetPrototype = "PainEffects";

    /// <summary>
    /// Minimum time between pain popups.
    /// </summary>
    [DataField]
    public TimeSpan MinimumPopupDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum time between pain popups.
    /// </summary>
    [DataField]
    public TimeSpan MaximumPopupDelay = TimeSpan.FromSeconds(40);
}
