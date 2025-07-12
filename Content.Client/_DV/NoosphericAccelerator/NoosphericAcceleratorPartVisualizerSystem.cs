using Content.Shared._DV.NoospericAccelerator.Components;
using Robust.Client.GameObjects;

namespace Content.Client._DV.NoosphericAccelerator;

public sealed class NoosphericAcceleratorPartVisualizerSystem : VisualizerSystem<NoosphericAcceleratorPartVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, NoosphericAcceleratorPartVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.Sprite.LayerMapTryGet(NoosphericAcceleratorVisualLayers.Unlit, out var index))
            return;

        if (!AppearanceSystem.TryGetData<NoosphericAcceleratorVisualState>(uid, NoosphericAcceleratorVisuals.VisualState, out var state, args.Component))
        {
            state = NoosphericAcceleratorVisualState.Unpowered;
        }

        if (state != NoosphericAcceleratorVisualState.Unpowered)
        {
            args.Sprite.LayerSetVisible(index, true);
            args.Sprite.LayerSetState(index, comp.StateBase + comp.StatesSuffixes[state]);
        }
        else
        {
            args.Sprite.LayerSetVisible(index, false);
        }
    }
}
