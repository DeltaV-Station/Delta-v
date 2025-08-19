using Content.Shared.Mobs;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client.DamageState;

public sealed class DamageStateVisualizerSystem : VisualizerSystem<DamageStateVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, DamageStateVisualsComponent component, ref AppearanceChangeEvent args)
    {
        var sprite = args.Sprite;

        if (sprite == null || !AppearanceSystem.TryGetData<MobState>(uid, MobStateVisuals.State, out var data, args.Component))
        {
            return;
        }

        if (!component.States.TryGetValue(data, out var layers))
        {
            return;
        }

        // Brain no worky rn so this was just easier.
        foreach (var key in new []{ DamageStateVisualLayers.Base, DamageStateVisualLayers.BaseUnshaded })
        {
<<<<<<< HEAD
            if (!sprite.LayerMapTryGet(key, out _)) continue;

            sprite.LayerSetVisible(key, false);
=======
            if (!SpriteSystem.LayerMapTryGet((uid, sprite), key, out _, false)) continue;

            SpriteSystem.LayerSetVisible((uid, sprite), key, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }

        foreach (var (key, state) in layers)
        {
            // Inheritance moment.
<<<<<<< HEAD
            if (!sprite.LayerMapTryGet(key, out _)) continue;

            sprite.LayerSetVisible(key, true);
            sprite.LayerSetState(key, state);
=======
            if (!SpriteSystem.LayerMapTryGet((uid, sprite), key, out _, false)) continue;

            SpriteSystem.LayerSetVisible((uid, sprite), key, true);
            SpriteSystem.LayerSetRsiState((uid, sprite), key, state);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }

        // So they don't draw over mobs anymore
        if (data == MobState.Dead)
        {
            if (sprite.DrawDepth > (int) DrawDepth.DeadMobs)
            {
                component.OriginalDrawDepth = sprite.DrawDepth;
<<<<<<< HEAD
                sprite.DrawDepth = (int) DrawDepth.DeadMobs;
=======
                SpriteSystem.SetDrawDepth((uid, sprite), (int)DrawDepth.DeadMobs);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
        }
        else if (component.OriginalDrawDepth != null)
        {
<<<<<<< HEAD
            sprite.DrawDepth = component.OriginalDrawDepth.Value;
=======
            SpriteSystem.SetDrawDepth((uid, sprite), component.OriginalDrawDepth.Value);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            component.OriginalDrawDepth = null;
        }
    }
}
