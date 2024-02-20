/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

using Robust.Client.GameObjects;
using System.Numerics;
using Content.Shared.DeltaV.Lamiae;

namespace Content.Client.DeltaV.Lamiae;

public sealed class ClientLamiaVisualSystem : VisualizerSystem<LamiaSegmentVisualsComponent>
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LamiaSegmentComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }
    private void OnAppearanceChange(EntityUid uid, LamiaSegmentComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null) return;

        if (AppearanceSystem.TryGetData<float>(uid, ScaleVisuals.Scale, out var scale) && TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.Scale = (new Vector2(scale, scale));
        }

        if (AppearanceSystem.TryGetData<bool>(uid, LamiaSegmentVisualLayers.Armor, out var worn)
            && AppearanceSystem.TryGetData<string>(uid, LamiaSegmentVisualLayers.ArmorRsi, out var path))
        {
            var valid = !string.IsNullOrWhiteSpace(path);
            if (valid)
            {
                args.Sprite.LayerSetRSI(LamiaSegmentVisualLayers.Armor, path);
            }
            args.Sprite.LayerSetVisible(LamiaSegmentVisualLayers.Armor, worn);
        }
    }
}
