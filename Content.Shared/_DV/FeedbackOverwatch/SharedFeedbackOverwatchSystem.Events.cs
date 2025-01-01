namespace Content.Shared._DV.FeedbackOverwatch;

public sealed partial class SharedFeedbackOverwatchSystem
{
    private void InitializeEvents()
    {
        // Subscribe to events that would be good for popups here. If it's a DeltaV specific system, do the subscriptions
        // in there (And import the SharedFeedbackOverwatchSystem to use SendPopup). If it's an upstream system try to
        // add the subscription here to avoid merge conflicts.
    }
}
