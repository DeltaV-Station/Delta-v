using Content.Shared.Light.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Light.Visualizers;

public sealed class LightBulbSystem : VisualizerSystem<LightBulbComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, LightBulbComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // update sprite state
        if (AppearanceSystem.TryGetData<LightBulbState>(uid, LightBulbVisuals.State, out var state, args.Component))
        {
            switch (state)
            {
                case LightBulbState.Normal:
<<<<<<< HEAD
                    args.Sprite.LayerSetState(LightBulbVisualLayers.Base, comp.NormalSpriteState);
                    break;
                case LightBulbState.Broken:
                    args.Sprite.LayerSetState(LightBulbVisualLayers.Base, comp.BrokenSpriteState);
                    break;
                case LightBulbState.Burned:
                    args.Sprite.LayerSetState(LightBulbVisualLayers.Base, comp.BurnedSpriteState);
=======
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), LightBulbVisualLayers.Base, comp.NormalSpriteState);
                    break;
                case LightBulbState.Broken:
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), LightBulbVisualLayers.Base, comp.BrokenSpriteState);
                    break;
                case LightBulbState.Burned:
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), LightBulbVisualLayers.Base, comp.BurnedSpriteState);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                    break;
            }
        }

        // also update sprites color
        if (AppearanceSystem.TryGetData<Color>(uid, LightBulbVisuals.Color, out var color, args.Component))
        {
<<<<<<< HEAD
            args.Sprite.Color = color;
=======
            SpriteSystem.SetColor((uid, args.Sprite), color);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}
