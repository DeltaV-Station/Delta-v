using Robust.Shared.GameStates;

namespace Content.Shared._DV.Reputation;

/// <summary>
/// Stores reputation-related data for mind entities.
/// Has a backup reputation value incase their PDA is deleted.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ReputationSystem))]
public sealed partial class MindReputationComponent : Component
{
    /// <summary>
    /// The traitor's PDA with <see cref="ContractsComponent"/>, might not always exist.
    /// </summary>
    [DataField]
    public EntityUid? Pda;

    [DataField]
    public int Reputation;
}
