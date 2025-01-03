using Content.Server._DV.StationEvents.NextEvent;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.StationEvents.NextEvent;

public sealed class NextEventSystem : EntitySystem
{
    /// <summary>
    ///     Updates the NextEventComponent with the provided id and time and returns the previously stored id.
    /// </summary>
    public EntProtoId? UpdateNextEvent(NextEventComponent component, EntProtoId newEventId, TimeSpan newEventTime)
    {
        EntProtoId? oldEventId = component.NextEventId; // Store components current NextEventId for return
        component.NextEventId = newEventId;
        component.NextEventTime = newEventTime;
        return oldEventId;
    }
}
