// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Client.DeltaV.Overlays;
using Content.Shared.DeltaV.Traits.Synthetic;
using Content.Shared.Mobs;
using Robust.Client.Graphics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Client.DeltaV.Traits.Synthetic;

public sealed class SynthSystem : SharedSynthSystem
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <summary>
    /// Sound that plays locally when you get hit by an EMP.
    /// </summary>
    private readonly SoundSpecifier _hallucinationSound = new SoundCollectionSpecifier("EmpHallucinations")
    {
        Params = new()
        {
            Variation = 0.05f,
            Volume = -6f
        }
    };

    private readonly StaticVisionOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SynthComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SynthComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<SynthComponent, LocalPlayerAttachedEvent>(OnLocalPlayerAttached);
        SubscribeLocalEvent<SynthComponent, LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
        SubscribeNetworkEvent<SynthGotEmpedEvent>(OnGotEmped);
    }

    private void OnComponentInit(EntityUid uid, SynthComponent component, ComponentInit args)
    {
        if (_playerManager.LocalEntity == uid)
            _overlayManager.AddOverlay(_overlay);
    }

    private void OnComponentShutdown(EntityUid uid, SynthComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
            _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnLocalPlayerAttached(EntityUid uid, SynthComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnLocalPlayerDetached(EntityUid uid, SynthComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    /// <summary>
    /// Triggered by the server when a synth is EMPed.
    /// </summary>>
    private void OnGotEmped(SynthGotEmpedEvent ev)
    {
        if (!_overlayManager.TryGetOverlay<StaticVisionOverlay>(out var overlay))
        {
            Log.Error($"Player is a synth but {nameof(StaticVisionOverlay)} is missing");
            return;
        }

        overlay.ReceiveStatic(15);
        _audio.PlayGlobal(_hallucinationSound, Filter.Local(), false);
    }

    /// <inheritdoc />
    protected override void OnMobStateChanged(EntityUid uid, SynthComponent component, MobStateChangedEvent args)
    {
        UpdateVisorLightState(uid, component);
    }
}
