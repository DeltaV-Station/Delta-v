using Content.Shared.DeltaV.Harpy;
using Robust.Client.GameObjects;
using Content.Shared.Humanoid;

namespace Content.Client.DeltaV.Harpy;

public sealed class HarpyVisualsSystem : VisualizerSystem<HarpyVisualsComponent>
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    protected override void OnAppearanceChange(EntityUid uid, HarpyVisualsComponent component,
        ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        _appearance.TryGetData(uid, HardsuitWings.Worn, out bool Worn);

        args.Sprite.LayerSetVisible(HumanoidVisualLayers.RArm, !Worn);
        args.Sprite.LayerSetVisible(HumanoidVisualLayers.Tail, !Worn);
    }
}
