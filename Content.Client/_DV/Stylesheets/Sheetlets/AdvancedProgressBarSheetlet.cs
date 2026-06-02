using Content.Client._DV.UserInterfaces.Controls;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._DV.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class AdvancedProgressBarSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet
{

    public override StyleRule[] GetRules(T sheet, object config)
    {
        var bgBox = new StyleBoxFlat(sheet.PrimaryPalette.BackgroundDark);
        var fgBox = new StyleBoxFlat(sheet.PrimaryPalette.BackgroundLight);
        
        var colorable = new StyleBoxFlat(Color.White);

        return
        [
            E<AdvancedProgressBar>()
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.BackgroundStyleClass))
                .Panel(bgBox),

            E<AdvancedProgressBar>()
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.ForegroundStyleClass))
                .Panel(fgBox),
            
            E<AdvancedProgressBar>()
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.ColorableStyleClass))
                .Panel(colorable),
        ];
    }
}
