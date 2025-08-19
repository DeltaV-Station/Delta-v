using Content.Shared.Power;
using Content.Shared.SMES;
using Robust.Client.GameObjects;

namespace Content.Client.Power.SMES;

public sealed class SmesVisualizerSystem : VisualizerSystem<SmesComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, SmesComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, SmesVisuals.LastChargeLevel, out var level, args.Component) || level == 0)
        {
<<<<<<< HEAD
            args.Sprite.LayerSetVisible(SmesVisualLayers.Charge, false);
        }
        else
        {
            args.Sprite.LayerSetVisible(SmesVisualLayers.Charge, true);
            args.Sprite.LayerSetState(SmesVisualLayers.Charge, $"{comp.ChargeOverlayPrefix}{level}");
=======
            SpriteSystem.LayerSetVisible((uid, args.Sprite), SmesVisualLayers.Charge, false);
        }
        else
        {
            SpriteSystem.LayerSetVisible((uid, args.Sprite), SmesVisualLayers.Charge, true);
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Charge, $"{comp.ChargeOverlayPrefix}{level}");
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }

        if (!AppearanceSystem.TryGetData<ChargeState>(uid, SmesVisuals.LastChargeState, out var state, args.Component))
            state = ChargeState.Still;

        switch (state)
        {
            case ChargeState.Still:
<<<<<<< HEAD
                args.Sprite.LayerSetState(SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}0");
                args.Sprite.LayerSetState(SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}1");
                break;
            case ChargeState.Charging:
                args.Sprite.LayerSetState(SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}1");
                args.Sprite.LayerSetState(SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}1");
                break;
            case ChargeState.Discharging:
                args.Sprite.LayerSetState(SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}0");
                args.Sprite.LayerSetState(SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}2");
=======
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}0");
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}1");
                break;
            case ChargeState.Charging:
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}1");
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}1");
                break;
            case ChargeState.Discharging:
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}0");
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}2");
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                break;
        }
    }
}

enum SmesVisualLayers : byte
{
    Input,
    Charge,
    Output,
}
