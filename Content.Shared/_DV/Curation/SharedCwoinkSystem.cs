using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Curation;

public abstract class SharedCwoinkSystem : EntitySystem
{
    // System users
    public static NetUserId SystemUserId { get; } = new NetUserId(Guid.Empty);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CwoinkTextMessage>(OnCwoinkTextMessage);
    }

    protected virtual void OnCwoinkTextMessage(CwoinkTextMessage message, EntitySessionEventArgs eventArgs)
    {
        // Specific side code in target.
    }

    protected void LogCwoink(CwoinkTextMessage message)
    {
    }
}

/// <summary>
///     Sent by the server to notify all clients when the webhook url is sent.
///     The webhook url itself is not and should not be sent.
/// </summary>
[Serializable, NetSerializable]
public sealed class CwoinkDiscordRelayUpdated : EntityEventArgs
{
    public bool DiscordRelayEnabled { get; }

    public CwoinkDiscordRelayUpdated(bool enabled)
    {
        DiscordRelayEnabled = enabled;
    }
}

/// <summary>
///     Sent by the client to notify the server when it begins or stops typing.
/// </summary>
[Serializable, NetSerializable]
public sealed class CwoinkClientTypingUpdated : EntityEventArgs
{
    public NetUserId Channel { get; }
    public bool Typing { get; }

    public CwoinkClientTypingUpdated(NetUserId channel, bool typing)
    {
        Channel = channel;
        Typing = typing;
    }
}

/// <summary>
///     Sent by server to notify admins when a player begins or stops typing.
/// </summary>
[Serializable, NetSerializable]
public sealed class CwoinkPlayerTypingUpdated : EntityEventArgs
{
    public NetUserId Channel { get; }
    public string PlayerName { get; }
    public bool Typing { get; }

    public CwoinkPlayerTypingUpdated(NetUserId channel, string playerName, bool typing)
    {
        Channel = channel;
        PlayerName = playerName;
        Typing = typing;
    }
}
