using System.Numerics;
using Content.Client.Ghost;
using Content.Shared.Stray.CustomGhosts;
using Robust.Client.GameObjects;
using Content.Shared.Ghost;

namespace Content.Client.Stray.CustomGhosts;

public sealed class CustomGhostVisualizer : VisualizerSystem<GhostComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, GhostComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if(args.Sprite == null) return;

        if (AppearanceSystem.TryGetData<string>(uid, CustomGhostAppearance.Sprite, out var rsiPath, args.Component))
        {
            args.Sprite.LayerSetRSI(0, rsiPath);
        }

        if(AppearanceSystem.TryGetData<float>(uid, CustomGhostAppearance.AlphaOverride, out var alpha, args.Component))
        {
            args.Sprite.Color = args.Sprite.Color.WithAlpha(alpha);
        }

        if (AppearanceSystem.TryGetData<Vector2>(uid, CustomGhostAppearance.SizeOverride, out var size, args.Component))
        {
            args.Sprite.Scale = size;
        }
    }
}
