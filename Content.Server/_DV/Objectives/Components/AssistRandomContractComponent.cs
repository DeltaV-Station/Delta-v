using Content.Server._DV.Objectives.Systems;
using Content.Shared.Whitelist;

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
    public LocId Description = "objective-condition-assist-traitor-description";

    /// <summary>
    /// Blacklist for objective entities that cannot be assisted with.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// The picked contract.
    /// </summary>
    [DataField]
    public EntityUid? Contract;
}
