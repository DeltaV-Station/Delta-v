using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.StationEvents;

public sealed partial class NextEventSystem : EntitySystem
{
    /// <summary>
    ///     Updates the NextEventComponent with the provided id and time and returns the previously stored id.
    /// </summary>
    public EntProtoId UpdateNextEvent(NextEventComponent component, EntProtoId newEventId, float newEventTime)
    {
        EntProtoId oldEventId = component.NextEventId; // Store components current NextEventId for return
        component.NextEventId = newEventId;
        component.NextEventTime = newEventTime;
        return oldEventId;
    }
}
