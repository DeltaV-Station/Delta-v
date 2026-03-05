using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Sheetlets;
using Content.Client.Stylesheets.SheetletConfigs;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._DV.AACTablet.UI;

// ReSharper disable InconsistentNaming
[CommonSheetlet]
public sealed class AACSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        // Create style boxes
        var controlBarBox = new StyleBoxFlat { BackgroundColor = Color.FromHex("#2a2a35") };
        var contentAreaBox = new StyleBoxFlat { BackgroundColor = Color.FromHex("#25252a") };
        var bufferDisplayBox = new StyleBoxFlat { BackgroundColor = Color.FromHex("#1a1a20") };
        bufferDisplayBox.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
        bufferDisplayBox.SetContentMarginOverride(StyleBox.Margin.Vertical, 4);

        var searchBarBox = new StyleBoxFlat { BackgroundColor = Color.FromHex("#2a2a35") };

        var transparentInputBox = new StyleBoxFlat
        {
            BackgroundColor = Color.Transparent,
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4
        };

        var rules = new List<StyleRule>
        {
            // AAC - Control Bar Background
            E<PanelContainer>()
                .Class("AacControlBar")
                .Panel(controlBarBox),

            // AAC - Content Area Background
            E<PanelContainer>()
                .Class("AacContentArea")
                .Panel(contentAreaBox),

            // AAC - Buffer Display (where combined phrases show)
            E<PanelContainer>()
                .Class("AacBufferDisplay")
                .Panel(bufferDisplayBox),

            // AAC - Search Bar
            E<PanelContainer>()
                .Class("AacSearchBar")
                .Panel(searchBarBox),

            // AAC - Buffer Text Style
            E<LineEdit>()
                .Class("AacBufferText")
                .Prop(LineEdit.StylePropertyStyleBox, transparentInputBox),

            // AAC - Search Input Style
            E<LineEdit>()
                .Class("AacSearchInput")
                .Prop(LineEdit.StylePropertyStyleBox, transparentInputBox),

            // AAC - Footer Text
            E<Label>()
                .Class("AacFooterText")
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(Color.FromHex("#757575")),
        };

        // Set the base button stylebox for AAC buttons
        rules.AddRange([
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class("SendButton")
                .Box(StyleBoxHelpers.BaseStyleBox(sheet)),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class("ClearButton")
                .Prop(Control.StylePropertyModulateSelf, Color.DarkRed),
        ]);

        return rules.ToArray();
    }
}
