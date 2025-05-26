using Content.Shared._DV.MedicalRecords;
using Content.Shared.Access.Systems;
using Content.Shared.StationRecords;
using Content.Server.StationRecords.Systems;

namespace Content.Server._DV.MedicalRecords;

public sealed class MedicalRecordsSystem : SharedMedicalRecordsSystem
{
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Retrieves the medical record associated with a patient. Creates default records if do not exist.
    /// </summary>
    /// <param name="patient">The unique identifier of the patient entity whose medical record is being retrieved.</param>
    /// <returns>The medical record of the specified patient.</returns>
    public MedicalRecord GetMedicalRecords(EntityUid patient)
    {
        EnsureComp<MedicalRecordComponent>(patient, out var comp);
        return comp.Record;
    }

    /// <summary>
    /// Updates the triage status of a patient and sets the command-level triage flag based on the actor's permissions.
    /// </summary>
    /// <param name="patient">The unique identifier of the patient whose status is being updated.</param>
    /// <param name="actor">The unique identifier of the actor attempting to change the patient's status.</param>
    /// <param name="newStatus">The new triage status to be assigned to the patient.</param>
    public void SetPatientStatus(EntityUid patient, EntityUid actor, TriageStatus newStatus)
    {
        var canSetCommandLevelTriage = CanSetCommandLevelTriage(actor);
        EnsureComp<MedicalRecordComponent>(patient, out var comp);

        if (comp.Record.IsCommandLevelTriage && !canSetCommandLevelTriage)
        {
            // actor is not permitted to override command level triage
            return;
        }
        comp.Record.Status = newStatus;
        comp.Record.IsCommandLevelTriage = canSetCommandLevelTriage;
        Dirty(patient, comp);
    }

    /// <summary>
    /// Updates the triage status of a patient and optionally marks it as command-level triage.
    /// </summary>
    /// <param name="patient">The unique identifier of the patient whose triage status is being updated.</param>
    /// <param name="newStatus">The new triage status to assign to the patient.</param>
    /// <param name="commandLevelFlag">A flag indicating if the triage status is considered command-level.</param>
    public void SetPatientStatus(EntityUid patient, TriageStatus newStatus, bool commandLevelFlag = false)
    {
        EnsureComp<MedicalRecordComponent>(patient, out var comp);
        comp.Record.Status = newStatus;
        comp.Record.IsCommandLevelTriage = commandLevelFlag;
        Dirty(patient, comp);
    }

    /// <summary>
    /// Uses the claimer's station record key to claim treatment of a patient
    /// </summary>
    /// <param name="patient">The unique identifier of the patient being claimed.</param>
    /// <param name="claimer">The unique identifier of the actor attempting to claim the patient.</param>
    public void ClaimPatient(EntityUid patient, EntityUid claimer)
    {
        _access.FindStationRecordKeys(claimer, out var keys);
        foreach (var key in keys)
        {
            var name = _records.RecordName(key);
            if (name == string.Empty)
                continue;

            EnsureComp<MedicalRecordComponent>(patient, out var comp);
            comp.Record.ClaimedName = name;
            Dirty(patient, comp);

            break;
        }
    }

    // todo placeholder
    public void UnclaimPatient(EntityUid patient)
    {
        EnsureComp<MedicalRecordComponent>(patient, out var comp);
        comp.Record.ClaimedName = null;
        Dirty(patient, comp);
    }

    /// <summary>
    /// Does the actor have permission to give command level triages?
    /// </summary>
    private bool CanSetCommandLevelTriage(EntityUid actor)
    {
        if (_idCard.TryFindIdCard(actor, out var card))
        {
            var tags = _access.FindAccessTags(actor);
            return tags.Contains("ChiefMedicalOfficer") || tags.Contains("Captain");
        }

        return false;
    }
}
