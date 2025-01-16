using Content.Shared._DV.FeedbackOverwatch;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;

namespace Content.Client._DV.FeedbackPopup;

/// <summary>
///     This handles getting feedback popup messages from the server and making a popup in the client.
///     Currently, this system can only support one window at a time.
/// </summary>
public sealed class FeedbackPopupUIController : UIController
{
    [Dependency] private readonly IClientNetManager _net = default!;

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
        _window.OnSubmitted += OnFeedbackSubmitted;
    }

    private void OnFeedbackSubmitted((LocId, string) args)
    {
        _net.ClientSendMessage(new FeedbackResponseMessage{ FeedbackName = Loc.GetString(args.Item1), FeedbackMessage = args.Item2 });
        _window?.Close();
    }
}
