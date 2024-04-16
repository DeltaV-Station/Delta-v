// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Client.Flash;
using Content.Shared.DeltaV.Traits.Synthetic;
using Robust.Client.Graphics;

namespace Content.Client.DeltaV.Traits.Synthetic;

public sealed class SynthSystem : SharedSynthSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<SynthGotEmpedEvent>(OnGotEmped);
    }

    private void OnGotEmped(SynthGotEmpedEvent ev)
    {
        var overlay = _overlayManager.GetOverlay<FlashOverlay>();
        overlay.ReceiveFlash(10);
    }
}

