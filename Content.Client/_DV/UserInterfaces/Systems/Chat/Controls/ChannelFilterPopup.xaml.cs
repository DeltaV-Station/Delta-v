using Content.Shared._DV.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed partial class ChannelFilterPopup
{
    public void SetAutoHighlights(string autoHighlights)
    {
        var anyAutohighlights = !string.IsNullOrWhiteSpace(autoHighlights);
        AutoHighlightEdit.Visible = anyAutohighlights;
        AutoHighlightLabel.Visible = anyAutohighlights;
        AutoHighlightEdit.TextRope = new Rope.Leaf(autoHighlights);
    }
}
