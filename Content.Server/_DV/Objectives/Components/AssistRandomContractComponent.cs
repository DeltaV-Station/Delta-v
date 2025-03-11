using Content.Server._DV.Objectives.Systems;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Picks a random contract of the target mind and requires that it be completed.
/// Requires that the objective has picked a target with at least 1 objective.
/// </summary>
[RegisterComponent, Access(typeof(AssistRandomContractSystem))]
public sealed partial class AssistRandomContractComponent : Component
{
    /// <summary>
    /// Description that gets "contract" passed.
    /// </summary>
    [DataField]
    public LocId Description = "objectives-condition-assist-traitor-description";

    /// <summary>
    /// The picked contract.
    /// </summary>
    [DataField]
    public EntityUid? Contract;
}
