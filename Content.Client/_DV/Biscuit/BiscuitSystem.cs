using Content.Shared._DV.Biscuit;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Biscuit;

public sealed class BiscuitSystem : VisualizerSystem<BiscuitVisualsComponent>
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    protected override void OnAppearanceChange(EntityUid uid, BiscuitVisualsComponent component,
        ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        _appearance.TryGetData(uid, BiscuitStatus.Cracked, out bool cracked);

        args.Sprite.LayerSetVisible(BiscuitVisualLayers.Top, !cracked);
    }
}

public enum BiscuitVisualLayers : byte
{
    Base,
    Top
}
