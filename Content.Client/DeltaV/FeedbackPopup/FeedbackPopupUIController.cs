using Content.Shared.DeltaV.FeedbackOverwatch;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.DeltaV.FeedbackPopup;

/// <summary>
///     This handles getting feedback popup messages from the server and making a popup in the client.
///     Currently, this system can only support one window at a time.
/// </summary>
public sealed class FeedbackPopupUIController : UIController
{
    private FeedbackPopupWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FeedbackPopupMessage>(OnFeedbackPopup);
    }

    private void OnFeedbackPopup(FeedbackPopupMessage msg, EntitySessionEventArgs args)
    {
        // If a window is already open, close it
        _window?.Close();

        _window = new FeedbackPopupWindow(msg.FeedbackPrototype);
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
    }
}
