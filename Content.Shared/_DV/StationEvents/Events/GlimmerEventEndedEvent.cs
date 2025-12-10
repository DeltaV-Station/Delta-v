namespace Content.Shared._DV.StationEvents.Events;

/// <summary>
/// After every glimmer event, it'll raise this event.
/// It's used for the sophic scribe to report glimmer changes.
/// </summary>
/// <param name="message">The message for the Sophic scribe to repeat.</param>
/// <param name="glimmerBurned">How much glimmer was burned through the event.</param>
[ByRefEvent]
public readonly struct GlimmerEventEndedEvent(string message, int glimmerBurned)
{
    public readonly string Message = message;
    public readonly int GlimmerBurned = glimmerBurned;
}
