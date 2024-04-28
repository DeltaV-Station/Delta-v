// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Client.DeltaV.Overlays;
using Content.Shared.DeltaV.Traits.Synthetic;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Robust.Client.Graphics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Client.DeltaV.Traits.Synthetic;

public sealed class SynthSystem : SharedSynthSystem
{
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SynthComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SynthComponent, RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeNetworkEvent<SynthGotEmpedEvent>(OnGotEmped);
    }

    /// <summary>
    /// Handles registering visor EMP effect overlay.
    /// </summary>
    private void OnComponentInit(EntityUid uid, SynthComponent component, ComponentInit args)
    {
        // yes, this never gets removed anymore during the round, and it gets added when someone "becomes a synth"
        // (e.g. by spawning) near you even if you are not a synth...
        // but it should hopefully not affect performance unless you are a synth.
        _overlayManager.AddOverlay(new StaticVisionOverlay());

    }

    /// <summary>
    /// Removes visor EMP effect overlay on round end.
    /// </summary>
    private void OnRoundRestartCleanup(EntityUid uid, SynthComponent component, RoundRestartCleanupEvent args)
    {
        _overlayManager.RemoveOverlay<StaticVisionOverlay>();
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
