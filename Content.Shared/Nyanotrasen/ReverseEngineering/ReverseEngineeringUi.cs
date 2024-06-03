using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.ReverseEngineering;

[Serializable, NetSerializable]
public enum ReverseEngineeringMachineUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ReverseEngineeringScanMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ReverseEngineeringSafetyMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ReverseEngineeringAutoScanMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ReverseEngineeringStopMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ReverseEngineeringEjectMessage : BoundUserInterfaceMessage;

/// <summary>
/// State updated when the values it uses changes to avoid creating it every frame.
/// </summary>
[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineState : BoundUserInterfaceState
{
    public readonly FormattedMessage ScanMessage;

    public ReverseEngineeringMachineState(FormattedMessage scanMessage)
    {
        ScanMessage = scanMessage;
    }
}

/// <summary>
// 3d6 + scanner bonus + danger bonus - item difficulty
/// </summary>
[Serializable, NetSerializable]
public enum ReverseEngineeringTickResult : byte
{
    Destruction, // 9 (only destroys if danger bonus is active, less the aversion bonus)
    Stagnation, // 10
    SuccessMinor, // 11-12
    SuccessAverage, // 13-15
    SuccessMajor, // 16-17
    InstantSuccess // 18
}
