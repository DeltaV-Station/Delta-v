using Content.Shared._DV.MedicalRecords; // DeltaV - Medical Records
using Content.Shared.Chemistry.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.HealthAnalyzerPlus;

/// <summary>
/// On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class HealthAnalyzerPlusScannedUserMessage : BoundUserInterfaceMessage
{
    public HealthAnalyzerPlusUiState State;

    public HealthAnalyzerPlusScannedUserMessage( HealthAnalyzerPlusUiState state )
    {
        State = state;
    }
}

/// <summary>
/// Contains the current state of a health analyzer control. Used only for the health analyzer plus.
/// </summary>
[Serializable, NetSerializable]
public struct HealthAnalyzerPlusUiState
{
    public readonly NetEntity? TargetEntity;
    public float Temperature;
    public float BloodLevel;
    public bool? ScanMode;
    public bool? Bleeding;
    public bool? Unrevivable;
    public MedicalRecord? MedicalRecord; // DeltaV - Medical Records
    public readonly Solution? BloodType;
    public readonly Solution? BloodSolution;

    public HealthAnalyzerPlusUiState() {}

    public HealthAnalyzerPlusUiState(NetEntity? targetEntity, float temperature, float bloodLevel, bool? scanMode, bool? bleeding, bool? unrevivable, Solution? bloodType, Solution? bloodSolution, MedicalRecord? medicalRecord = null)
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        Bleeding = bleeding;
        Unrevivable = unrevivable;
        BloodType = bloodType;
        BloodSolution = bloodSolution;
        MedicalRecord = medicalRecord; // DeltaV - Medical Records
    }
}

