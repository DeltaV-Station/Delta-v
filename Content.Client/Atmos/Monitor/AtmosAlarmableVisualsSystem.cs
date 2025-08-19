using Content.Shared.Atmos.Monitor;
using Content.Shared.Power;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.Atmos.Monitor;

public sealed class AtmosAlarmableVisualsSystem : VisualizerSystem<AtmosAlarmableVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, AtmosAlarmableVisualsComponent component, ref AppearanceChangeEvent args)
    {
<<<<<<< HEAD
        if (args.Sprite == null || !args.Sprite.LayerMapTryGet(component.LayerMap, out var layer))
=======
        if (args.Sprite == null || !SpriteSystem.LayerMapTryGet((uid, args.Sprite), component.LayerMap, out var layer, false))
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            return;

        if (!args.AppearanceData.TryGetValue(PowerDeviceVisuals.Powered, out var poweredObject) ||
            poweredObject is not bool powered)
        {
            return;
        }

        if (component.HideOnDepowered != null)
        {
            foreach (var visLayer in component.HideOnDepowered)
            {
<<<<<<< HEAD
                if (args.Sprite.LayerMapTryGet(visLayer, out var powerVisibilityLayer))
                    args.Sprite.LayerSetVisible(powerVisibilityLayer, powered);
=======
                if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), visLayer, out var powerVisibilityLayer, false))
                    SpriteSystem.LayerSetVisible((uid, args.Sprite), powerVisibilityLayer, powered);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
        }

        if (component.SetOnDepowered != null && !powered)
        {
            foreach (var (setLayer, powerState) in component.SetOnDepowered)
            {
<<<<<<< HEAD
                if (args.Sprite.LayerMapTryGet(setLayer, out var setStateLayer))
                    args.Sprite.LayerSetState(setStateLayer, new RSI.StateId(powerState));
=======
                if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), setLayer, out var setStateLayer, false))
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), setStateLayer, new RSI.StateId(powerState));
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
        }

        if (args.AppearanceData.TryGetValue(AtmosMonitorVisuals.AlarmType, out var alarmTypeObject)
            && alarmTypeObject is AtmosAlarmType alarmType
            && powered
            && component.AlarmStates.TryGetValue(alarmType, out var state))
        {
<<<<<<< HEAD
            args.Sprite.LayerSetState(layer, new RSI.StateId(state));
=======
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), layer, new RSI.StateId(state));
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}
