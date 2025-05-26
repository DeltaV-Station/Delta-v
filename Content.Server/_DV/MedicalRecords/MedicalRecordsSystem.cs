using Content.Shared._DV.MedicalRecords;
using Content.Shared.Access.Systems;
using Content.Shared.StationRecords;
using Content.Server.StationRecords.Systems;

namespace Content.Server._DV.MedicalRecords;

public sealed class MedicalRecordsSystem : SharedMedicalRecordsSystem
{
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

    public void SetPatientStatus(EntityUid patient, TriageStatus newStatus)
    {
        EnsureComp<MedicalRecordComponent>(patient, out var comp);
        comp.Record.Status = newStatus;
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
}
