using Content.Shared._DV.Abilities.Kitsune;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Kitsune;

public sealed class KitsuneFoxSystem : VisualizerSystem<KitsuneFoxComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, KitsuneFoxComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite)
            return;

        if (!AppearanceSystem.TryGetData<Color>(uid, KitsuneColorVisuals.Color, out var color, args.Component))
            return;

        if (sprite.LayerMapTryGet(KitsuneColorVisuals.Layer, out var layer))
            sprite.LayerSetColor(layer, color);
    }
}
