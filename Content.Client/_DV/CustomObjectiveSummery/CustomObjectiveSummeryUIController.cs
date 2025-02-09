using Content.Shared._DV.CustomObjectiveSummery;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;

namespace Content.Client._DV.CustomObjectiveSummery;

public sealed class CustomObjectiveSummeryUIController : UIController
{
    [Dependency] private readonly IClientNetManager _net = default!;

    private CustomObjectiveSummeryWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CustomObjectiveSummeryOpenMessage>(OnCustomObjectiveSummeryOpen);
    }

    private void OnCustomObjectiveSummeryOpen(CustomObjectiveSummeryOpenMessage msg, EntitySessionEventArgs args)
    {
        OpenWindow();
    }

    public void OpenWindow()
    {
        // If a window is already open, close it
        _window?.Close();

        _window = new CustomObjectiveSummeryWindow();
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
        _window.OnSubmitted += OnFeedbackSubmitted;
    }

    private void OnFeedbackSubmitted(string args)
    {
        var msg = new CustomObjectiveClientSetObjective
        {
            Summery = args,
        };
        _net.ClientSendMessage(msg);
        _window?.Close();
    }
}
