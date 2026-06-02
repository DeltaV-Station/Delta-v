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
        var colorable = new StyleBoxFlat(Color.White);

        return
        [
            E<AdvancedProgressBar>()
                .ParentOf(E<PanelContainer>())
                .Panel(colorable),
            
            E<AdvancedProgressBar>()
                .Prop(AdvancedProgressBar.StylePropertyBackgroundColor, sheet.PrimaryPalette.BackgroundDark)
                .Prop(AdvancedProgressBar.StylePropertyForegroundColor, sheet.PrimaryPalette.Base),
            
            E<AdvancedProgressBar>()
                .Pseudo(AdvancedProgressBar.StylePseudoClassLeftToRight)
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.StyleClassForegroundPanelContainer))
                .HorizontalAlignment(Control.HAlignment.Left)
                .VerticalAlignment(Control.VAlignment.Stretch),
            
            E<AdvancedProgressBar>()
                .Pseudo(AdvancedProgressBar.StylePseudoClassRightToLeft)
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.StyleClassForegroundPanelContainer))
                .HorizontalAlignment(Control.HAlignment.Right)
                .VerticalAlignment(Control.VAlignment.Stretch),
            
            E<AdvancedProgressBar>()
                .Pseudo(AdvancedProgressBar.StylePseudoClassTopToBottom)
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.StyleClassForegroundPanelContainer))
                .HorizontalAlignment(Control.HAlignment.Stretch)
                .VerticalAlignment(Control.VAlignment.Top),
            
            E<AdvancedProgressBar>()
                .Pseudo(AdvancedProgressBar.StylePseudoClassBottomToTop)
                .ParentOf(E<PanelContainer>().Class(AdvancedProgressBar.StyleClassForegroundPanelContainer))
                .HorizontalAlignment(Control.HAlignment.Stretch)
                .VerticalAlignment(Control.VAlignment.Bottom),
        ];
    }
}
