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

        SubscribeLocalEvent<AfterGeneralRecordCreatedEvent>(OnGeneralRecordCreated);
    }

    private void OnGeneralRecordCreated(AfterGeneralRecordCreatedEvent evt)
    {
        _records.AddRecordEntry(evt.Key, new MedicalRecord());
        _records.Synchronize(evt.Key);
    }

    public void SetStatus(StationRecordKey key, MedicalRecord record)
    {
        var name = _records.RecordName(key);
        if (name != string.Empty)
            UpdateMedicalRecords(name, record);

        _records.AddRecordEntry(key, record);
        _records.Synchronize(key);
    }

    public MedicalRecord? GetMedicalRecords(EntityUid patient)
    {
        _access.FindStationRecordKeys(patient, out var keys);
        foreach (var key in keys)
        {
            if (_records.TryGetRecord<MedicalRecord>(key, out var record))
                return record;
        }
        foreach (var key in keys)
        {
            var medicalRecord = new MedicalRecord();
            SetStatus(key, medicalRecord);
            return medicalRecord;
        }
        return null;
    }

    public StationRecordKey? GetMedicalRecordsKey(EntityUid patient)
    {
        _access.FindStationRecordKeys(patient, out var keys);
        foreach (var key in keys)
        {
            if (_records.TryGetRecord<MedicalRecord>(key, out var record))
                return key;
        }
        foreach (var key in keys)
        {
            SetStatus(key, new MedicalRecord());
            return key;
        }
        return null;
    }

    public void SetPatientStatus(StationRecordKey patient, TriageStatus status)
    {
        if (_records.TryGetRecord<MedicalRecord>(patient, out var record))
        {
            // Triage status exists, update it.
            SetStatus(patient, record with { Status = status });
        }
        else
        {
            // Triage status doesn't exist, create it.
            SetStatus(patient, new MedicalRecord { Status = status });
        }
    }

    public void ClaimPatient(StationRecordKey patient, EntityUid claimer)
    {
        _access.FindStationRecordKeys(claimer, out var keys);
        foreach (var key in keys)
        {
            var name = _records.RecordName(key);
            if (name == string.Empty)
                continue;

            if (_records.TryGetRecord<MedicalRecord>(patient, out var record))
            {
                if (record.ClaimedName == name)
                    continue;

                // Update the status
                SetStatus(patient, record with { ClaimedName = name });
            }
            else
            {
                // Create the status
                SetStatus(patient, new MedicalRecord { ClaimedName = name });
            }
            break;
        }
    }

    // todo placeholder
    public void UnclaimPatient(StationRecordKey patient)
    {
        if (_records.TryGetRecord<MedicalRecord>(patient, out var record))
        {
            // Update the status
            SetStatus(patient, record with { ClaimedName = null });
        }
    }
}
