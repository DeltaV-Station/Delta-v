using Content.Server._DV.Objectives.Systems;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Requires a certain number of reputation to roll an objective.
/// </summary>
[RegisterComponent, Access(typeof(ReputationConditionSystem))]
public sealed partial class ReputationConditionComponent : Component
{
    /// <summary>
    /// The required reputation.
    /// </summary>
    [DataField(required: true)]
    public int Reputation;
}
