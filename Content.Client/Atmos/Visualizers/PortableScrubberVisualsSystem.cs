using Robust.Client.GameObjects;
using Content.Shared.Atmos.Visuals;
using Content.Client.Power;

namespace Content.Client.Atmos.Visualizers
{
<<<<<<< HEAD
    /// <summary>
    /// Controls client-side visuals for portable scrubbers.
    /// </summary>
    public sealed class PortableScrubberSystem : VisualizerSystem<PortableScrubberVisualsComponent>
=======
    protected override void OnAppearanceChange(EntityUid uid, PortableScrubberVisualsComponent component, ref AppearanceChangeEvent args)
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    {
        protected override void OnAppearanceChange(EntityUid uid, PortableScrubberVisualsComponent component, ref AppearanceChangeEvent args)
        {
<<<<<<< HEAD
            if (args.Sprite == null)
                return;

            if (AppearanceSystem.TryGetData<bool>(uid, PortableScrubberVisuals.IsFull, out var isFull, args.Component)
                && AppearanceSystem.TryGetData<bool>(uid, PortableScrubberVisuals.IsRunning, out var isRunning, args.Component))
            {
                var runningState = isRunning ? component.RunningState : component.IdleState;
                args.Sprite.LayerSetState(PortableScrubberVisualLayers.IsRunning, runningState);

                var fullState = isFull ? component.FullState : component.ReadyState;
                args.Sprite.LayerSetState(PowerDeviceVisualLayers.Powered, fullState);
            }

            if (AppearanceSystem.TryGetData<bool>(uid, PortableScrubberVisuals.IsDraining, out var isDraining, args.Component))
            {
                args.Sprite.LayerSetVisible(PortableScrubberVisualLayers.IsDraining, isDraining);
            }
=======
            var runningState = isRunning ? component.RunningState : component.IdleState;
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), PortableScrubberVisualLayers.IsRunning, runningState);

            var fullState = isFull ? component.FullState : component.ReadyState;
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), PowerDeviceVisualLayers.Powered, fullState);
        }

        if (AppearanceSystem.TryGetData<bool>(uid, PortableScrubberVisuals.IsDraining, out var isDraining, args.Component))
        {
            SpriteSystem.LayerSetVisible((uid, args.Sprite), PortableScrubberVisualLayers.IsDraining, isDraining);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}
public enum PortableScrubberVisualLayers : byte
{
    IsRunning,

    IsDraining
}
