using Content.Shared.Power;
using Robust.Client.GameObjects;

namespace Content.Client.PowerCell;

public sealed class PowerChargerVisualizerSystem : VisualizerSystem<PowerChargerVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, PowerChargerVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Update base item
        if (AppearanceSystem.TryGetData<bool>(uid, CellVisual.Occupied, out var occupied, args.Component) && occupied)
        {
            // TODO: don't throw if it doesn't have a full state
<<<<<<< HEAD
            args.Sprite.LayerSetState(PowerChargerVisualLayers.Base, comp.OccupiedState);
        }
        else
        {
            args.Sprite.LayerSetState(PowerChargerVisualLayers.Base, comp.EmptyState);
=======
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), PowerChargerVisualLayers.Base, comp.OccupiedState);
        }
        else
        {
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), PowerChargerVisualLayers.Base, comp.EmptyState);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }

        // Update lighting
        if (AppearanceSystem.TryGetData<CellChargerStatus>(uid, CellVisual.Light, out var status, args.Component)
        &&  comp.LightStates.TryGetValue(status, out var lightState))
        {
<<<<<<< HEAD
            args.Sprite.LayerSetState(PowerChargerVisualLayers.Light, lightState);
            args.Sprite.LayerSetVisible(PowerChargerVisualLayers.Light, true);
        }
        else
            // 
            args.Sprite.LayerSetVisible(PowerChargerVisualLayers.Light, false);
=======
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), PowerChargerVisualLayers.Light, lightState);
            SpriteSystem.LayerSetVisible((uid, args.Sprite), PowerChargerVisualLayers.Light, true);
        }
        else
            SpriteSystem.LayerSetVisible((uid, args.Sprite), PowerChargerVisualLayers.Light, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    }
}

enum PowerChargerVisualLayers : byte
{
    Base,
    Light,
}
