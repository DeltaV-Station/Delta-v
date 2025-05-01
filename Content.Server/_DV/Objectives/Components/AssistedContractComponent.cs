using Content.Server._DV.Objectives.Systems;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Tracks which assist objectives are active for a given contract.
/// If this contract is failed all of the assists are failed too.
/// </summary>
[RegisterComponent, Access(typeof(AssistRandomContractSystem))]
public sealed partial class AssistedContractComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Assisting = new();
}
