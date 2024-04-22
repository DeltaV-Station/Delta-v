// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Shared.Traits;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Traits.Synthetic;

/// <summary>
/// Set players' blood to coolant, and is used to notify them of ion storms
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SynthComponent : Component
{
    /// <summary>
    /// The chance that the synth is alerted of an ion storm
    /// </summary>
    [DataField]
    public float AlertChance = 0.3f;

    /// <summary>
    /// The EntityUid of the visor if present, used for controlling the visor light and such.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? VisorUid;

    /// <summary>
    /// If set true only the eyes glow, not the side LEDs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool EyeGlowOnly;
}
