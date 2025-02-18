namespace Content.Client.UserInterface.Systems.Chat.Widgets;

public partial class ChatBox
{
    private void OnNewHighlights(string highlights)
    {
        _controller.UpdateHighlights(highlights);
    }

    private void OnHighlightsReceived(string highlights)
    {
        ChatInput.FilterButton.Popup.SetHighlights(highlights);
    }
}
