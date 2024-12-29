using Content.Shared.DeltaV.FeedbackOverwatch;
using Robust.Shared.Configuration;

namespace Content.Client.DeltaV.FeedbackPopup;

/// <summary>
///     This handles getting feedback popup messages from the server and making a popup in the client.
///     Currently, this system can only support one window at a time.
/// </summary>
public sealed class FeedbackPopupSystem : EntitySystem
{
    private FeedbackPopupWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FeedbackPopupMessage>(OnFeedbackPopup);
    }

    private void OnFeedbackPopup(FeedbackPopupMessage msg)
    {
        // If a window is already open, close it
        _window?.Close();

        _window = new FeedbackPopupWindow(msg.FeedbackPrototype);
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
    }
}
