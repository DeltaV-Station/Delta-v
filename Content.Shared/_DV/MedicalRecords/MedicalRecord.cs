using Robust.Shared.Serialization;

namespace Content.Shared._DV.MedicalRecords;

/// <summary>
/// Status used in Medical Records.
///
/// Dnr - Do Not Resuscitate in effect, do not treat
/// Low - Treatment is low priority or not possible
/// Normal - Default value, normal treatment.
/// High - High priority
/// </summary>
public enum TriageStatus : byte
{
    Dnr,
    Low,
    Normal,
    High,
}

/// <summary>
/// Medical record for a crewmember.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public sealed partial record MedicalRecord
{
    /// <summary>
    /// Status of the person
    /// </summary>
    public TriageStatus Status = TriageStatus.Normal;

    /// <summary>
    /// The name of the doctor who has claimed care of this patient
    /// </summary>
    public string? ClaimedName;
}
