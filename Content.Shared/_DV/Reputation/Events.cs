using Content.Shared.Mind;

namespace Content.Shared._DV.Reputation;

/// <summary>
/// Event that gets raised on an objective after it has been added and taken.
/// </summary>
[ByRefEvent]
public record struct ContractTakenEvent(Entity<ContractsComponent> Contracts, Entity<MindComponent> Mind);

/// <summary>
/// Event that gets raised on an objective after it becomes impossible to completed.
/// It gets deleted afterwards.
/// </summary>
[ByRefEvent]
public record struct ContractFailedEvent(Entity<ContractsComponent> Contracts);

/// <summary>
/// Event that gets raised on an objective after it has been completed.
/// </summary>
[ByRefEvent]
public record struct ContractCompletedEvent(Entity<ContractsComponent> Contracts);
