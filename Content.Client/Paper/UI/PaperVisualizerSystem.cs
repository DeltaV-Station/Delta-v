using Robust.Client.GameObjects;

using static Content.Shared.Paper.PaperComponent;

namespace Content.Client.Paper.UI;

public sealed class PaperVisualizerSystem : VisualizerSystem<PaperVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, PaperVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<PaperStatus>(uid, PaperVisuals.Status, out var writingStatus, args.Component))
<<<<<<< HEAD
            args.Sprite.LayerSetVisible(PaperVisualLayers.Writing, writingStatus == PaperStatus.Written);
=======
            SpriteSystem.LayerSetVisible((uid, args.Sprite), PaperVisualLayers.Writing, writingStatus == PaperStatus.Written);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

        if (AppearanceSystem.TryGetData<string>(uid, PaperVisuals.Stamp, out var stampState, args.Component))
        {
            if (stampState != string.Empty)
            {
<<<<<<< HEAD
                args.Sprite.LayerSetState(PaperVisualLayers.Stamp, stampState);
                args.Sprite.LayerSetVisible(PaperVisualLayers.Stamp, true);
            }
            else
            {
                args.Sprite.LayerSetVisible(PaperVisualLayers.Stamp, false);
=======
                SpriteSystem.LayerSetRsiState((uid, args.Sprite), PaperVisualLayers.Stamp, stampState);
                SpriteSystem.LayerSetVisible((uid, args.Sprite), PaperVisualLayers.Stamp, true);
            }
            else
            {
                SpriteSystem.LayerSetVisible((uid, args.Sprite), PaperVisualLayers.Stamp, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }

        }
    }
}

public enum PaperVisualLayers
{
    Stamp,
    Writing
}
