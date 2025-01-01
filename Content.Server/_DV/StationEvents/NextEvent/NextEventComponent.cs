using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.StationEvents.NextEvent;

[RegisterComponent, Access(typeof(NextEventSystem))]
[AutoGenerateComponentPause]
public sealed partial class NextEventComponent : Component
{
    /// <summary>
    /// Id of the next event that will be run by EventManagerSystem.
    /// </summary>
    [DataField]
    public EntProtoId? NextEventId;

    /// <summary>
    /// Round time of the scheduler's next station event.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextEventTime;
}
