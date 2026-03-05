using Robust.Shared.Serialization;

namespace Content.Shared._DV.MedicalRecords;

/// <summary>
/// Status used in Medical Records.
///
/// None - the default value
/// Minor - minor injuries, may be able to assist in own care
/// Delayed - serious injuries, but not expected to deteroriate within a few minutes
/// Immediate - can be helped with immediate intervention and transport
/// Expectant - unlikely to survive or already dead
/// </summary>
public enum TriageStatus : byte
{
    None,
    Minor,
    Delayed,
    Immediate,
    Expectant,
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
    public TriageStatus Status = TriageStatus.None;

    /// <summary>
    /// The name of the doctor who has claimed care of this patient
    /// </summary>
    public string? ClaimedName;
}
