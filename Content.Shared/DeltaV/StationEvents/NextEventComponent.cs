using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.StationEvents;

[RegisterComponent, Access(typeof(NextEventSystem))]
public sealed partial class NextEventComponent : Component
{
    /// <summary>
    ///     Id of the next event that will be run by EventManagerSystem.
    /// </summary>
    [DataField]
    public EntProtoId NextEventId { get; set; } = string.Empty;

    /// <summary>
    ///     Round time of the scheduler's next station event.
    /// </summary>
    [DataField]
    public TimeSpan NextEventTime;
}
