using Content.Shared._DV.MedicalRecords; // DeltaV - Medical Records
using Content.Shared.Chemistry.Components; // DeltaV - Health Analyzer Plus
using Robust.Shared.Serialization;

namespace Content.Shared.MedicalScanner;

/// <summary>
/// On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class HealthAnalyzerScannedUserMessage : BoundUserInterfaceMessage
{
    public HealthAnalyzerUiState State;

    public HealthAnalyzerScannedUserMessage(HealthAnalyzerUiState state)
    {
        State = state;
    }
}

/// <summary>
/// Contains the current state of a health analyzer control. Used for the health analyzer and cryo pod.
/// </summary>
[Serializable, NetSerializable]
public struct HealthAnalyzerUiState
{
    public readonly NetEntity? TargetEntity;
    public float Temperature;
    public float BloodLevel;
    public bool? ScanMode;
    public bool? Bleeding;
    public bool? Unrevivable;
    public readonly Solution? BloodSolution; // DeltaV - Health Analyzer Plus
    public MedicalRecord? MedicalRecord; // DeltaV - Medical Records

    public HealthAnalyzerUiState() {}

    public HealthAnalyzerUiState(NetEntity? targetEntity, float temperature, float bloodLevel, bool? scanMode, bool? bleeding, bool? unrevivable, Solution? bloodSolution, MedicalRecord? medicalRecord = null) // DeltaV - Health Analyzer Plus, Medical Records
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        Bleeding = bleeding;
        Unrevivable = unrevivable;
        BloodSolution = bloodSolution; // DeltaV - Health Analyzer Plus
        MedicalRecord = medicalRecord; // DeltaV - Medical Records
    }
}

