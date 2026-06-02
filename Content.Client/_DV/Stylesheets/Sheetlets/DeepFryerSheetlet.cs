using Content.Client._DV.Kitchen.UI.Controls;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Sheetlets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._DV.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class DeepFryerSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet, IButtonConfig, IIconConfig
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        var itemWrapperBox = new StyleBoxFlat(sheet.SecondaryPalette.BackgroundDark.WithAlpha(0.5f));
        var buttonBox = new StyleBoxFlat(sheet.SecondaryPalette.BackgroundLight.WithAlpha(0.90f));

        var rules = new List<StyleRule>
        {
            E<FryerBaskets>()
                .Panel(itemWrapperBox),

            E<BoxContainer>()
                .Class(FryerBaskets.ContentContainerStyleClass)
                .Prop(BoxContainer.StylePropertySeparation, 5),

            E<FryerItemButton>()
                .Box(buttonBox),

            E<FryerItemButton>()
                .ParentOf(E<BoxContainer>())
                .Prop(BoxContainer.StylePropertySeparation, 5),
        };

        ButtonSheetlet<T>
            .MakeButtonRules<FryerItemButton>(rules, sheet.SecondaryPalette, null);

        return rules.ToArray();
    }
}
