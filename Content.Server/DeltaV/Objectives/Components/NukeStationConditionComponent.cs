﻿using Content.Server.DeltaV.Objectives.Systems;

namespace Content.Server.DeltaV.Objectives.Components;

/// <summary>
///     For nuclear operatives trying to nuke the station. Should only be completed if the correct station is exploded.
/// </summary>
[RegisterComponent, Access(typeof(NukeStationConditionSystem))]
public sealed partial class NukeStationConditionComponent : Component;
