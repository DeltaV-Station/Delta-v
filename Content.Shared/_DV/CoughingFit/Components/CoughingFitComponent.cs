using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CoughingFit.Components;

/// <summary>
/// This is used for the coughing fit trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class CoughingFitComponent : Component
{
    /// <summary>
    /// The maximum time between fits.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public TimeSpan MaxTimeBetweenFits = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The minimum time between fits.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public TimeSpan MinTimeBetweenFits = TimeSpan.FromMinutes(5); // 5-10 minutes between fits

    /// <summary>
    /// The maximum duration of fits.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public TimeSpan MaxDurationOfFit = TimeSpan.FromSeconds(50);

    /// <summary>
    /// The minimum duration of fits.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public TimeSpan MinDurationOfFit = TimeSpan.FromSeconds(15); // 15-50 second coughing fits

    /// <summary>
    /// Next time fit happens.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextFitTime = TimeSpan.Zero;
}
