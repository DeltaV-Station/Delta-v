/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

using Content.Server.DeltaV.ParadoxAnomaly.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.ParadoxAnomaly.Components;

/// <summary>
/// Creates a random paradox anomaly and tranfers mind to it when taken by a player.
/// </summary>
[RegisterComponent, Access(typeof(ParadoxAnomalySystem))]
public sealed partial class ParadoxAnomalySpawnerComponent : Component
{
    /// <summary>
    /// Antag game rule to start for the paradox anomaly.
    /// </summary>
    [DataField]
    public EntProtoId Rule = "ParadoxAnomaly";
}
