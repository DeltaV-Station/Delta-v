using Content.Server._DV.Objectives.Systems;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Requires that you kidnap a person using syndicate fultons.
/// They can be bought back at a cargo ordering console.
/// </summary>
[RegisterComponent, Access(typeof(RansomConditionSystem))]
public sealed partial class RansomConditionComponent : Component;
