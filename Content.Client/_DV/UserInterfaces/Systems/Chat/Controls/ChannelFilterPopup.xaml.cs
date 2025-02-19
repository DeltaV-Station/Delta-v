using Content.Shared._DV.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed partial class ChannelFilterPopup
{
    public event Action<string>? OnNewHighlights;

    private void InitializeChatHighlights()
    {
        HighlightButton.OnPressed += HighlightsEntered;

        HighlightEdit.Placeholder = new Rope.Leaf(Loc.GetString("hud-chatbox-highlights-placeholder"));

        // Load highlights if any were saved.
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        var highlights = cfg.GetCVar(DCCVars.ChatHighlights);

        if (!string.IsNullOrEmpty(highlights))
        {
            SetHighlights(highlights);
        }
    }

    public void SetHighlights(string highlights)
    {
        HighlightEdit.TextRope = new Rope.Leaf(highlights);
    }

    private void HighlightsEntered(ButtonEventArgs args)
    {
        OnNewHighlights?.Invoke(Rope.Collapse(HighlightEdit.TextRope));
    }
}
