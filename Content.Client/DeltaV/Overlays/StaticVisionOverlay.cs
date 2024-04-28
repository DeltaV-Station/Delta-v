// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Shared.DeltaV.Traits.Synthetic;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.DeltaV.Overlays;

/// <summary>
/// Overlay for synths and the like that simulates seeing static. Works like the flash one.
/// </summary>
public sealed class StaticVisionOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => _needsScreenTexture;
    private bool _needsScreenTexture = false;
    private readonly ShaderInstance _shader;
    private double _startTime = -1;
    private double _lastsFor = 1;

    public StaticVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypeManager.Index<ShaderPrototype>("StaticVisionEffect").Instance().Duplicate();
    }

    public void ReceiveStatic(double duration)
    {
        _startTime = _gameTiming.CurTime.TotalSeconds;
        _lastsFor = duration;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        // we only request the screen texture if we REALLY need it, which is when the player is controlling a synth
        if (_playerManager.LocalEntity is not {Valid: true} player
            || !_entityManager.HasComponent<SynthComponent>(player)
            || !_entityManager.TryGetComponent(player, out EyeComponent? eyeComp)
            || args.Viewport.Eye != eyeComp.Eye)
        {
            _needsScreenTexture = false;
            return;
        }

        _needsScreenTexture = true;
        if (ScreenTexture == null) // yes, this is a frame of delay but i don't think it matters
            return;

        var percentComplete = (float) ((_gameTiming.CurTime.TotalSeconds - _startTime) / _lastsFor);
        if (percentComplete >= 1.0f)
            return;

        var worldHandle = args.WorldHandle;
        worldHandle.UseShader(_shader);
        _shader.SetParameter("percentComplete", percentComplete);
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        var viewport = args.WorldBounds;
        worldHandle.SetTransform(Matrix3.Identity);
        worldHandle.DrawRect(viewport, Color.White);
    }
}
