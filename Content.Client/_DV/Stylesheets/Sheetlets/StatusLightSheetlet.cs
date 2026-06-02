using Content.Client._DV.UserInterfaces.Controls;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Robust.Client.Animations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Animations;

using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._DV.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class StatusLightSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet
{
    private const string DefaultBaseLightResPath = "/Textures/Interface/WireHacking/light_off_base.svg.96dpi.png";
    private const string DefaultActiveLightResPath = "/Textures/Interface/WireHacking/light_on_base.svg.96dpi.png";
    
    private readonly Animation _blinkingFastAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.2),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = nameof(Control.Modulate),
                InterpolationMode = AnimationInterpolationMode.Linear,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.Transparent, 0.1f),
                    new AnimationTrackProperty.KeyFrame(Color.White, 0.1f)
                }
            }
        }
    };

    private readonly Animation _blinkingSlowAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.8),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = nameof(Control.Modulate),
                InterpolationMode = AnimationInterpolationMode.Linear,
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, 0.3f),
                    new AnimationTrackProperty.KeyFrame(Color.Transparent, 0.1f),
                    new AnimationTrackProperty.KeyFrame(Color.Transparent, 0.3f),
                    new AnimationTrackProperty.KeyFrame(Color.White, 0.1f),
                }
            }
        }
    };


    public override StyleRule[] GetRules(T sheet, object config)
    {
        return
        [
            E<StatusLight>()
                .Prop(StatusLight.StylePropertyBlinkingAnimation, false)
                .Prop(StatusLight.StylePropertyActiveColor, Color.Green.WithAlpha(0.3f))
                .Prop(StatusLight.StylePropertyBaseColor, Color.FromHex("#202020")),
            
            E<StatusLight>()
                .ParentOf(E<TextureRect>().Class(StatusLight.StyleClassBaseLight))
                .Prop(TextureRect.StylePropertyTexture, ResCache.GetTexture(DefaultBaseLightResPath)),
            
            E<StatusLight>()
                .ParentOf(E<TextureRect>().Class(StatusLight.StyleClassActiveLight))
                .Prop(TextureRect.StylePropertyTexture, ResCache.GetTexture(DefaultActiveLightResPath)),
            
            /* Animation Classes */
            E<StatusLight>()
                .Class(StatusLight.StyleClassFastBlinking)
                .Pseudo(StatusLight.StylePseudoClassIsOn)
                .Prop(StatusLight.StylePropertyBlinkingAnimation, _blinkingFastAnimation),
            
            E<StatusLight>()
                .Class(StatusLight.StyleClassSlowBlinking)
                .Pseudo(StatusLight.StylePseudoClassIsOn)
                .Prop(StatusLight.StylePropertyBlinkingAnimation, _blinkingSlowAnimation)
        ];
    }
}
