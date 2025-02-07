using Robust.Shared.Serialization;

namespace Content.Shared._DV.Reputation;

[Serializable, NetSerializable]
public enum ContractsUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ContractsState : BoundUserInterfaceState;
// TODO

/// <summary>
/// Accept a contract with offerings index.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContractsAcceptMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

/// <summary>
/// Complete a contract whose objective has been completed, with slot index.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContractsCompleteMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

/// <summary>
/// Rejects an active contract with slots index.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContractsRejectMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

/// <summary>
/// Picks more offerings if there are none available.
/// Failsafe incase of bad RNG giving you nothing.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContractsRescanMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class PdaShowContractsMessage : BoundUserInterfaceMessage;
