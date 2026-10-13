using Content.Client.Resources;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._DV.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class DVLineEditSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var tinyUnicode = ResCache.GetFont("/Fonts/_DV/TinyUnicode.ttf", size: 24);

        return
        [
            E<LineEdit>()
                .Class("comms-console-display")
                .Font(tinyUnicode),
        ];
    }
}
