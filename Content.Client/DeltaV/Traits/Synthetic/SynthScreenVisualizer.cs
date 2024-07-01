// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Shared.DeltaV.Traits.Synthetic;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.DeltaV.Traits.Synthetic;

public sealed class SynthScreenVisualizer : VisualizerSystem<SynthComponent>
{
    private readonly SpriteSpecifier.Rsi _glowyVisorSprite =
        new(new("DeltaV/Mobs/Customization/Synthetic/visor_glow.rsi"), "glow");
    private readonly SpriteSpecifier.Rsi _notGlowyVisorSprite =
        new(new("DeltaV/Mobs/Customization/Synthetic/visor_glow.rsi"), "off");
    private readonly SpriteSpecifier.Rsi _glowyEyesSprite =
        new(new("DeltaV/Mobs/Customization/Synthetic/visor_glow.rsi"), "eyesonly_glow");
    private readonly SpriteSpecifier.Rsi _notGlowyEyesSprite =
        new(new("Mobs/Customization/eyes.rsi"), "eyes");

    protected override void OnAppearanceChange(EntityUid uid, SynthComponent component, ref AppearanceChangeEvent args)
    {
        if (component.VisorUid is null // can't get this if you don't have a visor
            || !TryComp<SpriteComponent>(uid, out var sprite)
            || !AppearanceSystem.TryGetData<Color>(uid, SynthVisorVisuals.EyeColor, out var eyeColor, args.Component)
            || !AppearanceSystem.TryGetData<bool>(uid, SynthVisorVisuals.Alive, out var alive, args.Component)
            || !sprite.LayerMapTryGet("eyes", out var eyesLayer))
        {
            return;
        }

        sprite.LayerSetColor(eyesLayer, eyeColor);

        if (alive)
        {
            // eyes and the side lamp (if present) glow if you're alive
            sprite.LayerSetSprite(eyesLayer, component.EyeGlowOnly ? _glowyEyesSprite : _glowyVisorSprite);
            sprite.LayerSetShader(eyesLayer, "unshaded");
        }
        else
        {
            // and they don't if you're dead!!
            // unset shader to make it not glow
            sprite.LayerSetSprite(eyesLayer, component.EyeGlowOnly ? _notGlowyEyesSprite : _notGlowyVisorSprite);
            sprite.LayerSetShader(eyesLayer, null, null);
        }
    }
}
