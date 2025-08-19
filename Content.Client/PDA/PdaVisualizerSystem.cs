using Content.Shared.Light;
using Content.Shared.PDA;
using Robust.Client.GameObjects;

namespace Content.Client.PDA;

public sealed class PdaVisualizerSystem : VisualizerSystem<PdaVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, PdaVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, PdaVisuals.PdaType, out var pdaType, args.Component))
<<<<<<< HEAD
            args.Sprite.LayerSetState(PdaVisualLayers.Base, pdaType);

        if (AppearanceSystem.TryGetData<bool>(uid, UnpoweredFlashlightVisuals.LightOn, out var isFlashlightOn, args.Component))
            args.Sprite.LayerSetVisible(PdaVisualLayers.Flashlight, isFlashlightOn);

        if (AppearanceSystem.TryGetData<bool>(uid, PdaVisuals.IdCardInserted, out var isCardInserted, args.Component))
            args.Sprite.LayerSetVisible(PdaVisualLayers.IdLight, isCardInserted);
=======
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), PdaVisualLayers.Base, pdaType);

        if (AppearanceSystem.TryGetData<bool>(uid, UnpoweredFlashlightVisuals.LightOn, out var isFlashlightOn, args.Component))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), PdaVisualLayers.Flashlight, isFlashlightOn);

        if (AppearanceSystem.TryGetData<bool>(uid, PdaVisuals.IdCardInserted, out var isCardInserted, args.Component))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), PdaVisualLayers.IdLight, isCardInserted);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    }

    public enum PdaVisualLayers : byte
    {
        Base,
        Flashlight,
        IdLight
    }
}
