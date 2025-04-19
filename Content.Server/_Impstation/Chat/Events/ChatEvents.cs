namespace Content.Server.Chat.Events;

/// <summary>
///     Imp
///     Raised on an entity when it emotes, whether through emote wheel, hotkeys, /me, @ or * in textbox, etc.
/// </summary>
public sealed class EntityEmotedEvent : EntityEventArgs
{
    public readonly EntityUid Source;
    public readonly string Message;

    public EntityEmotedEvent(EntityUid source, string message)
    {
        Source = source;
        Message = message;
    }
}
