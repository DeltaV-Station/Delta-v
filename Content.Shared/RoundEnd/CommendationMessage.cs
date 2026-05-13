using Robust.Shared.Serialization;

namespace Content.Shared.RoundEnd;

/// <summary>
/// Sent from client to server when a player gives a commendation (RP like) to another player at round end.
/// </summary>
[NetSerializable, Serializable]
public sealed class CommendPlayerMessage : EntityEventArgs
{
    /// <summary>
    /// The OOC name (C-key / username) of the player receiving the commendation.
    /// </summary>
    public string ReceiverOOCName { get; }

    public CommendPlayerMessage(string receiverOocName)
    {
        ReceiverOOCName = receiverOocName;
    }
}

/// <summary>
/// Sent from server to client to confirm that the commendation was recorded.
/// </summary>
[NetSerializable, Serializable]
public sealed class CommendationSentMessage : EntityEventArgs
{
    public bool Success { get; }
    public string? Error { get; }

    public CommendationSentMessage(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }
}
