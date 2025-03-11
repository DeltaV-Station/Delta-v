using Content.Server.Objectives.Systems;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Sets the target for <see cref="TargetObjectiveComponent"/> to a random traitor who has enough reputation.
/// </summary>
[RegisterComponent]
public sealed partial class PickRandomTraitorComponent : Component
{
    /// <summary>
    /// Minimum reputation to require, or 0 for no requirement.
    /// </summary>
    [DataField]
    public int MinReputation;
}
