using Content.Client.Light.Components;
using Content.Shared.Light.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Light.EntitySystems;

public sealed class EmergencyLightSystem : VisualizerSystem<EmergencyLightComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, EmergencyLightComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<bool>(uid, EmergencyLightVisuals.On, out var on, args.Component))
            on = false;

<<<<<<< HEAD
        args.Sprite.LayerSetVisible(EmergencyLightVisualLayers.LightOff, !on);
        args.Sprite.LayerSetVisible(EmergencyLightVisualLayers.LightOn, on);

        if (AppearanceSystem.TryGetData<Color>(uid, EmergencyLightVisuals.Color, out var color, args.Component))
        {
            args.Sprite.LayerSetColor(EmergencyLightVisualLayers.LightOn, color);
            args.Sprite.LayerSetColor(EmergencyLightVisualLayers.LightOff, color);
=======
        SpriteSystem.LayerSetVisible((uid, args.Sprite), EmergencyLightVisualLayers.LightOff, !on);
        SpriteSystem.LayerSetVisible((uid, args.Sprite), EmergencyLightVisualLayers.LightOn, on);

        if (AppearanceSystem.TryGetData<Color>(uid, EmergencyLightVisuals.Color, out var color, args.Component))
        {
            SpriteSystem.LayerSetColor((uid, args.Sprite), EmergencyLightVisualLayers.LightOn, color);
            SpriteSystem.LayerSetColor((uid, args.Sprite), EmergencyLightVisualLayers.LightOff, color);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}
