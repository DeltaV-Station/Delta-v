using Robust.Shared.Serialization;

namespace Content.Shared.MachineLinking;

[Serializable, NetSerializable]
public enum SignalTimerUiKey : byte
{
    Key
}

/// <summary>
/// Represents a SignalTimerComponent state that can be sent to the client
/// </summary>
[Serializable, NetSerializable]
public sealed class SignalTimerBoundUserInterfaceState : BoundUserInterfaceState
{
    public string CurrentText;
    // Begin Monolith
    // public string CurrentDelayMinutes;
    // public string CurrentDelaySeconds;
    public TimeSpan CurrentDelay;
    // End Monolith
    public bool ShowText;
    public TimeSpan TriggerTime;
    public bool TimerStarted;
    public bool HasAccess;

    public SignalTimerBoundUserInterfaceState(string currentText,
        // Begin Monolith
        // string currentDelayMinutes,
        // string currentDelaySeconds,
        TimeSpan currentDelay,
        // End Monolith
        bool showText,
        TimeSpan triggerTime,
        bool timerStarted,
        bool hasAccess)
    {
        CurrentText = currentText;
        // Begin Monolith
        // CurrentDelayMinutes = currentDelayMinutes;
        // CurrentDelaySeconds = currentDelaySeconds;
        CurrentDelay = currentDelay;
        // End Monolith
        ShowText = showText;
        TriggerTime = triggerTime;
        TimerStarted = timerStarted;
        HasAccess = hasAccess;
    }
}

[Serializable, NetSerializable]
public sealed class SignalTimerTextChangedMessage : BoundUserInterfaceMessage
{
    public string Text { get; }

    public SignalTimerTextChangedMessage(string text)
    {
        Text = text;
    }
}

[Serializable, NetSerializable]
public sealed class SignalTimerDelayChangedMessage : BoundUserInterfaceMessage
{
    public TimeSpan Delay { get; }
    public SignalTimerDelayChangedMessage(TimeSpan delay)
    {
        Delay = delay;
    }
}

[Serializable, NetSerializable]
public sealed class SignalTimerStartMessage : BoundUserInterfaceMessage
{

}
