using Content.Shared.Wires;
using Robust.Client.GameObjects;

namespace Content.Client.Wires.Visualizers
{
    public sealed class WiresVisualizerSystem : VisualizerSystem<WiresVisualsComponent>
    {
        protected override void OnAppearanceChange(EntityUid uid, WiresVisualsComponent component, ref AppearanceChangeEvent args)
        {
            if (args.Sprite == null)
                return;

<<<<<<< HEAD
            var layer = args.Sprite.LayerMapReserveBlank(WiresVisualLayers.MaintenancePanel);
=======
            var layer = SpriteSystem.LayerMapReserve((uid, args.Sprite), WiresVisualLayers.MaintenancePanel);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

            if(args.AppearanceData.TryGetValue(WiresVisuals.MaintenancePanelState, out var panelStateObject) &&
                panelStateObject is bool panelState)
            {
<<<<<<< HEAD
                args.Sprite.LayerSetVisible(layer, panelState);
=======
                SpriteSystem.LayerSetVisible((uid, args.Sprite), layer, panelState);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
            else
            {
                //Mainly for spawn window
<<<<<<< HEAD
                args.Sprite.LayerSetVisible(layer, false);
=======
                SpriteSystem.LayerSetVisible((uid, args.Sprite), layer, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
        }
    }

    public enum WiresVisualLayers : byte
    {
        MaintenancePanel
    }
}
