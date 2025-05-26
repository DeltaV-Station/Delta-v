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

    public MedicalRecord? GetMedicalRecords(EntityUid patient)
    {
        EnsureComp<MedicalRecordComponent>(patient, out var comp);
        return comp.Record;
    }

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
