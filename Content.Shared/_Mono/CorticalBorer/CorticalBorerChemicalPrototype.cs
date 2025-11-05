// SPDX-FileCopyrightText: 2025 Coenx-flex
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._Mono.CorticalBorer;

[Prototype("borerChemical")]
public sealed partial class CorticalBorerChemicalPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Chemical cost per u of reagent
    /// </summary>
    [DataField]
    public int Cost { get; set; } = 5;

    /// <summary>
    /// Reagent to inject into host
    /// </summary>
    [DataField]
    public string Reagent { get; set; } = "";
}
