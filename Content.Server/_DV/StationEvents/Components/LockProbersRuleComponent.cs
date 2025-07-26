using Content.Server._DV.StationEvents.Events;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.StationEvents.Components;

/// <summary>
/// Locks all the probers on the station and turns them into bombs.
/// </summary>
[RegisterComponent, Access(typeof(GlimmerMobRule))]
public sealed partial class LockProbersRuleComponent : Component { }
