using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Reputation;

/// <summary>
/// Ties a store entity (pda or uplink implant) to a mind's <see cref="ContractsComponent"/>.
/// Limits what it can buy with <c>ReputationCondition</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ReputationSystem))]
[AutoGenerateComponentState]
public sealed partial class StoreContractsComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Mind;

    /// <summary>
    /// Action given for uplink implants.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionOpenContractsImplant";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionId;
}
