using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._DV.Traits.UI;

[CommonSheetlet]
public sealed class TraitsSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        // Color palette
        // sorry but the default ColorPalette just sucks in terms of ligher/darker colors
        var bgDark = Color.FromHex("#1a1a22");
        var bgMedium = Color.FromHex("#22222a");
        var bgLight = Color.FromHex("#2a2a35");
        var bgLighter = Color.FromHex("#32323e");
        var textPrimary = Color.FromHex("#e0e0e0");
        var textSecondary = Color.FromHex("#a0a0a0");
        var textMuted = Color.FromHex("#707070");
        var accentGreen = Color.FromHex("#4ade80");
        var accentYellow = Color.FromHex("#fbbf24");
        var accentRed = Color.FromHex("#f87171");
        var accentBlue = Color.FromHex("#60a5fa");

        // StyleBoxes
        var headerPanelBox = new StyleBoxFlat
        {
            BackgroundColor = bgLight,
            BorderColor = bgLighter,
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
        headerPanelBox.SetContentMarginOverride(StyleBox.Margin.All, 0);

        var searchBarBox = new StyleBoxFlat { BackgroundColor = bgMedium };
        searchBarBox.SetContentMarginOverride(StyleBox.Margin.All, 0);

        var searchInputBox = new StyleBoxFlat
        {
            BackgroundColor = bgDark,
            ContentMarginLeftOverride = 8,
            ContentMarginRightOverride = 8
        };

        var footerPanelBox = new StyleBoxFlat
        {
            BackgroundColor = bgMedium,
            BorderColor = bgLighter,
            BorderThickness = new Thickness(0, 1, 0, 0)
        };

        var categoryHeaderBox = new StyleBoxFlat { BackgroundColor = bgLight };
        categoryHeaderBox.SetContentMarginOverride(StyleBox.Margin.All, 0);

        var categoryHeaderButtonBox = new StyleBoxFlat { BackgroundColor = Color.Transparent };
        categoryHeaderButtonBox.SetContentMarginOverride(StyleBox.Margin.All, 0);

        var categoryContentBox = new StyleBoxFlat { BackgroundColor = bgMedium };

        var categoryAccentBox = new StyleBoxFlat { BackgroundColor = accentBlue };

        var entryPanelBox = new StyleBoxFlat
        {
            BackgroundColor = bgLight,
            BorderColor = bgLighter,
            BorderThickness = new Thickness(1)
        };
        entryPanelBox.SetContentMarginOverride(StyleBox.Margin.All, 0);

        var entrySelectedBox = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#2a3a4a"),
            BorderColor = accentBlue,
            BorderThickness = new Thickness(1, 1, 1, 1)
        };
        entrySelectedBox.SetContentMarginOverride(StyleBox.Margin.All, 0);

        var progressBarBgBox = new StyleBoxFlat
        {
            BackgroundColor = bgDark,
            BorderColor = bgLighter,
            BorderThickness = new Thickness(1)
        };

        var progressBarFillFull = new StyleBoxFlat { BackgroundColor = accentGreen };
        var progressBarFillPartial = new StyleBoxFlat { BackgroundColor = accentYellow };
        var progressBarFillLow = new StyleBoxFlat { BackgroundColor = accentRed };
        var progressBarFillEmpty = new StyleBoxFlat { BackgroundColor = bgDark };

        var rules = new List<StyleRule>
        {
            // ===== HEADER PANEL =====
            E<PanelContainer>()
                .Class("TraitsHeaderPanel")
                .Panel(headerPanelBox),

            E<Label>()
                .Class("TraitsTitleLabel")
                .Font(sheet.BaseFont.GetFont(14))
                .FontColor(textPrimary),

            E<Label>()
                .Class("TraitsSubtitleLabel")
                .Font(sheet.BaseFont.GetFont(11))
                .FontColor(textSecondary),

            E<Label>()
                .Class("TraitsStatLabel")
                .Font(sheet.BaseFont.GetFont(12))
                .FontColor(accentBlue),

            // ===== PROGRESS BAR =====
            E<PanelContainer>()
                .Class("TraitsProgressBarBg")
                .Panel(progressBarBgBox),

            E<PanelContainer>()
                .Class("TraitsProgressBarFill")
                .Panel(progressBarFillFull),

            E<PanelContainer>()
                .Class("TraitsProgressBarFull")
                .Panel(progressBarFillFull),

            E<PanelContainer>()
                .Class("TraitsProgressBarPartial")
                .Panel(progressBarFillPartial),

            E<PanelContainer>()
                .Class("TraitsProgressBarLow")
                .Panel(progressBarFillLow),

            E<PanelContainer>()
                .Class("TraitsProgressBarEmpty")
                .Panel(progressBarFillEmpty),

            // ===== SEARCH BAR =====
            E<PanelContainer>()
                .Class("TraitsSearchBar")
                .Panel(searchBarBox),

            E<LineEdit>()
                .Class("TraitsSearchInput")
                .Prop(LineEdit.StylePropertyStyleBox, searchInputBox),

            // ===== FOOTER =====
            E<PanelContainer>()
                .Class("TraitsFooterPanel")
                .Panel(footerPanelBox),

            E<Label>()
                .Class("TraitsFooterText")
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(textMuted),

            // ===== CATEGORY HEADER =====
            E<PanelContainer>()
                .Class("TraitsCategoryHeader")
                .Panel(categoryHeaderBox),

            E<Button>()
                .Class("TraitsCategoryHeaderButton")
                .Prop(Button.StylePropertyStyleBox, categoryHeaderButtonBox),

            E<Label>()
                .Class("TraitsCategoryExpandIcon")
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(textSecondary),

            E<Label>()
                .Class("TraitsCategoryNameLabel")
                .Font(sheet.BaseFont.GetFont(12))
                .FontColor(textPrimary),

            E<Label>()
                .Class("TraitsCategoryStatsLabel")
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(textSecondary),

            E<Label>()
                .Class("TraitsCategoryPointsLabel")
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(textMuted),

            // ===== CATEGORY ACCENT =====
            E<PanelContainer>()
                .Class("TraitsCategoryAccent")
                .Panel(categoryAccentBox),

            // ===== CATEGORY CONTENT =====
            E<PanelContainer>()
                .Class("TraitsCategoryContent")
                .Panel(categoryContentBox),

            // ===== TRAIT ENTRY =====
            E<PanelContainer>()
                .Class("TraitsEntryPanel")
                .Panel(entryPanelBox),

            E<PanelContainer>()
                .Class("TraitsEntryPanel")
                .Class("TraitsEntrySelected")
                .Panel(entrySelectedBox),

            E<Label>()
                .Class("TraitsEntryNameLabel")
                .Font(sheet.BaseFont.GetFont(11))
                .FontColor(textPrimary),

            E<Label>()
                .Class("TraitsEntryCostLabel")
                .Font(sheet.BaseFont.GetFont(11)),

            E<RichTextLabel>()
                .Class("TraitsEntryDescriptionLabel")
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(textSecondary),
        };

        return rules.ToArray();
    }
}
