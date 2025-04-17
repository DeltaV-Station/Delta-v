using Robust.Shared.Serialization;

namespace Content.Shared._DV.MedicalRecords;

[Serializable, NetSerializable]
public sealed class HealthAnalyzerTriageStatusMessage(TriageStatus triageStatus) : BoundUserInterfaceMessage
{
    public readonly TriageStatus TriageStatus = triageStatus;
}

[Serializable, NetSerializable]
public sealed class HealthAnalyzerTriageClaimMessage : BoundUserInterfaceMessage;
