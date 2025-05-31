#nullable enable
using Content.Shared._DV.Curation;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Client._DV.Curation.Systems;

public sealed class CwoinkSystem : SharedCwoinkSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public event EventHandler<SharedCwoinkSystem.CwoinkTextMessage>? OnCwoinkTextMessageRecieved;
    private (TimeSpan Timestamp, bool Typing) _lastTypingUpdateSent;

    protected override void OnCwoinkTextMessage(SharedCwoinkSystem.CwoinkTextMessage message, EntitySessionEventArgs eventArgs)
    {
        OnCwoinkTextMessageRecieved?.Invoke(this, message);
    }

    public void Send(NetUserId channelId, string text, bool playSound, bool adminOnly)
    {
        // Reuse the channel ID as the 'true sender'.
        // Server will ignore this and if someone makes it not ignore this (which is bad, allows impersonation!!!), that will help.
        RaiseNetworkEvent(new SharedCwoinkSystem.CwoinkTextMessage(channelId, channelId, text, playSound: playSound, adminOnly: adminOnly));
        SendInputTextUpdated(channelId, false);
    }

    public void SendInputTextUpdated(NetUserId channel, bool typing)
    {
        if (_lastTypingUpdateSent.Typing == typing &&
            _lastTypingUpdateSent.Timestamp + TimeSpan.FromSeconds(1) > _timing.RealTime)
        {
            return;
        }

        _lastTypingUpdateSent = (_timing.RealTime, typing);
        RaiseNetworkEvent(new CwoinkClientTypingUpdated(channel, typing));
    }
}
