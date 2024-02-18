/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Content.Shared.DeltaV.Biscuit;
using Robust.Client.GameObjects;

namespace Content.Client.DeltaV.Biscuit;

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
