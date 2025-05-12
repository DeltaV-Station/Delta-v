using Content.Shared._DV.MedicalRecords; // DeltaV - Medical Records
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared.Body.Part;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;
using Content.Shared.Chemistry.Components;
namespace Content.Shared._Shitmed.Medical.HealthAnalyzer;

// Base message that contains common data for all Modes
[Serializable, NetSerializable]
public abstract class HealthAnalyzerBaseMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity;
    public readonly float Temperature;
    public readonly float BloodLevel;
    public readonly bool? ScanMode;
    public readonly HealthAnalyzerMode ActiveMode;
    public Dictionary<TargetBodyPart, WoundableSeverity>? Body;

    public HealthAnalyzerBaseMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        HealthAnalyzerMode activeMode,
        Dictionary<TargetBodyPart, WoundableSeverity>? body)
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        ActiveMode = activeMode;
        Body = body;
    }
}

// Body Mode message
[Serializable, NetSerializable]
public sealed class HealthAnalyzerBodyMessage : HealthAnalyzerBaseMessage
{
    public readonly bool? Bleeding;
    public readonly bool? Unrevivable;
    public readonly NetEntity? SelectedPart;
    public readonly Dictionary<NetEntity, List<WoundableTraumaData>> Traumas;
    public readonly Dictionary<NetEntity, FixedPoint2> NervePainFeels;
    public MedicalRecord? MedicalRecord; // DeltaV - Medical Records

    public HealthAnalyzerBodyMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        bool? bleeding,
        bool? unrevivable,
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<NetEntity, List<WoundableTraumaData>> traumas,
        Dictionary<NetEntity, FixedPoint2> nervePainFeels,
        MedicalRecord? medicalRecord, // DeltaV
        NetEntity? selectedPart = null)
        : base(targetEntity, temperature, bloodLevel, scanMode, HealthAnalyzerMode.Body, body)
    {
        Bleeding = bleeding;
        Unrevivable = unrevivable;
        SelectedPart = selectedPart;
        Traumas = traumas;
        NervePainFeels = nervePainFeels;
        MedicalRecord = medicalRecord; // DeltaV - Medical Records
    }
}

// Organs Mode message
[Serializable, NetSerializable]
public sealed class HealthAnalyzerOrgansMessage : HealthAnalyzerBaseMessage
{
    public readonly Dictionary<NetEntity, OrganTraumaData> Organs;

    public HealthAnalyzerOrgansMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<NetEntity, OrganTraumaData> organs)
        : base(targetEntity, temperature, bloodLevel, scanMode, HealthAnalyzerMode.Organs, body)
    {
        Organs = organs;
    }
}

// Chemicals Mode message
[Serializable, NetSerializable]
public sealed class HealthAnalyzerChemicalsMessage : HealthAnalyzerBaseMessage
{
    public readonly Dictionary<NetEntity, Solution> Solutions;

    public HealthAnalyzerChemicalsMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<NetEntity, Solution> solutions)
        : base(targetEntity, temperature, bloodLevel, scanMode, HealthAnalyzerMode.Chemicals, body)
    {
        Solutions = solutions;
    }
}

// Mode selection message (from client to server)
[Serializable, NetSerializable]
public sealed class HealthAnalyzerModeSelectedMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? Owner;
    public readonly HealthAnalyzerMode Mode;

    public HealthAnalyzerModeSelectedMessage(NetEntity owner, HealthAnalyzerMode mode)
    {
        Owner = owner;
        Mode = mode;
    }
}

// Part selection message (from client to server)
[Serializable, NetSerializable]
public sealed class HealthAnalyzerPartSelectedMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? Owner;
    public readonly TargetBodyPart? BodyPart;

    public HealthAnalyzerPartSelectedMessage(NetEntity? owner, TargetBodyPart? bodyPart)
    {
        Owner = owner;
        BodyPart = bodyPart;
    }
}

[Serializable, NetSerializable]
public struct WoundableTraumaData
{
    public string Name;
    public string TraumaType;
    public FixedPoint2 Severity;
    public string? SeverityString; // Used mostly in Bone Damage traumas to keep track of the secondary severity.
    public (BodyPartType, BodyPartSymmetry)? TargetType;

    public WoundableTraumaData(string name,
        string traumaType,
        FixedPoint2 severity,
        string? severityString = null,
        (BodyPartType, BodyPartSymmetry)? targetType = null)
    {
        Name = name;
        TraumaType = traumaType;
        Severity = severity;
        SeverityString = severityString;
        TargetType = targetType;
    }
}

// Supporting data structures
[Serializable, NetSerializable]
public struct OrganTraumaData
{
    public FixedPoint2 Integrity;
    public FixedPoint2 IntegrityCap;
    public OrganSeverity Severity;
    public List<(string Name, FixedPoint2 Value)> Modifiers;

    public OrganTraumaData(FixedPoint2 integrity,
        FixedPoint2 integrityCap,
        OrganSeverity severity,
        List<(string Name, FixedPoint2 Value)> modifiers)
    {
        Integrity = integrity;
        IntegrityCap = integrityCap;
        Severity = severity;
        Modifiers = modifiers;
    }
}

[Serializable, NetSerializable]
public enum HealthAnalyzerMode
{
    Body,
    Organs,
    Chemicals
}
