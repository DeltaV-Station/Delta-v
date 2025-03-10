using Content.Client._DV.Kitsune;
using Content.Shared._DV.Abilities.Kitsune;
using Robust.Client.GameObjects;
using static Robust.Client.GameObjects.SpriteComponent;

namespace Content.Client._DV.Kitsune;


public sealed class KitsuneSystem : VisualizerSystem<KitsuneComponent>
{

    protected override void OnAppearanceChange(EntityUid uid, KitsuneComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
        {
            return;
        }

        if (!AppearanceSystem.TryGetData<Color>(uid, KitsuneColor.Color, out var color, args.Component))
        {
            return;
        }

        if (!TryComp<SpriteComponent>(uid, out var sprite))
        {
            return;
        }

        foreach (var spriteLayer in args.Sprite.AllLayers)
        {
            if (spriteLayer.RsiState.Name == "kitsune_fox_body")
                spriteLayer.Color = color;
        }

    }


}
