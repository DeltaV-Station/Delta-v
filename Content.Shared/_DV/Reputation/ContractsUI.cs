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
/// Rejects a contract offering with offerings index.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContractsRejectMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

[Serializable, NetSerializable]
public sealed class PdaShowContractsMessage : BoundUserInterfaceMessage;
