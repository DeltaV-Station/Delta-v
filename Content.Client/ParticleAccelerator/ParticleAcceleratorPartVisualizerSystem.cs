using System.Linq;
using Content.Shared.Singularity.Components;
using Robust.Client.GameObjects;

namespace Content.Client.ParticleAccelerator;

public sealed class ParticleAcceleratorPartVisualizerSystem : VisualizerSystem<ParticleAcceleratorPartVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, ParticleAcceleratorPartVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

<<<<<<< HEAD
        if (!args.Sprite.LayerMapTryGet(ParticleAcceleratorVisualLayers.Unlit, out var index))
=======
        if (!SpriteSystem.LayerMapTryGet((uid, args.Sprite), ParticleAcceleratorVisualLayers.Unlit, out var index, false))
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            return;

        if (!AppearanceSystem.TryGetData<ParticleAcceleratorVisualState>(uid, ParticleAcceleratorVisuals.VisualState, out var state, args.Component))
        {
            state = ParticleAcceleratorVisualState.Unpowered;
        }

        if (state != ParticleAcceleratorVisualState.Unpowered)
        {
<<<<<<< HEAD
            args.Sprite.LayerSetVisible(index, true);
            args.Sprite.LayerSetState(index, comp.StateBase + comp.StatesSuffixes[state]);
        }
        else
        {
            args.Sprite.LayerSetVisible(index, false);
=======
            SpriteSystem.LayerSetVisible((uid, args.Sprite), index, true);
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), index, comp.StateBase + comp.StatesSuffixes[state]);
        }
        else
        {
            SpriteSystem.LayerSetVisible((uid, args.Sprite), index, false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}
