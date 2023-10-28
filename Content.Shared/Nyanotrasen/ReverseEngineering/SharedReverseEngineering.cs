using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.ReverseEngineering;

[Serializable, NetSerializable]
public enum ReverseEngineeringMachineUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineScanButtonPressedMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineSafetyButtonToggledMessage : BoundUserInterfaceMessage
{
    public bool Safety;

    public ReverseEngineeringMachineSafetyButtonToggledMessage(bool safety)
    {
        Safety = safety;
    }
}

[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineAutoScanButtonToggledMessage : BoundUserInterfaceMessage
{
    public bool AutoScan;

    public ReverseEngineeringMachineAutoScanButtonToggledMessage(bool autoScan)
    {
        AutoScan = autoScan;
    }
}

[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineStopButtonPressedMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineEjectButtonPressedMessage : BoundUserInterfaceMessage
{
}


[Serializable, NetSerializable]
public sealed class ReverseEngineeringMachineScanUpdateState : BoundUserInterfaceState
{
    public NetEntity? Target;

    public bool CanScan;

    public FormattedMessage? ScanReport;

    public bool Scanning;

    public bool Safety;

    public bool AutoProbe;

    public int TotalProgress;

    public TimeSpan TimeRemaining;

    public TimeSpan TotalTime;

    public ReverseEngineeringMachineScanUpdateState(NetEntity? target, bool canScan,
        FormattedMessage? scanReport, bool scanning, bool safety, bool autoProbe, int totalProgress, TimeSpan timeRemaining, TimeSpan totalTime)
    {
        Target = target;
        CanScan = canScan;

        ScanReport = scanReport;

        Scanning = scanning;
        Safety = safety;
        AutoProbe = autoProbe;
        TotalProgress = totalProgress;
        TimeRemaining = timeRemaining;
        TotalTime = totalTime;
    }
}

/// <summary>
// 3d6 + scanner bonus + danger bonus - item difficulty
/// </summary>
[Serializable, NetSerializable]
public enum ReverseEngineeringTickResult : byte
{
    Destruction, // 9 (only destroys if danger bonus is active, effectively 8 since aversion bonus is always 1)
    Stagnation, // 10
    SuccessMinor, // 11-12
    SuccessAverage, // 13-15
    SuccessMajor, // 16-17
    InstantSuccess // 18
}

[Serializable, NetSerializable]
public enum ReverseEngineeringVisuals : byte
{
    ChamberOpen,
}
