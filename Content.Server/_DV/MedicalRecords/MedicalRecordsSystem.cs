using Content.Shared._DV.MedicalRecords;
using Content.Shared.Access.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.StationRecords;
using Content.Server.StationRecords.Systems;
using Robust.Shared.Timing;

namespace Content.Server._DV.MedicalRecords;

public sealed class MedicalRecordsSystem : SharedMedicalRecordsSystem
{
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float ExpirationTime = 300f; // 5 minutes in seconds

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
            {
                // Check if expired when accessed
                if (record.LastUpdated != null && (_timing.CurTime - record.LastUpdated.Value).TotalSeconds >= ExpirationTime)
                {
                    record = record with { Status = TriageStatus.None, ClaimedName = null, LastUpdated = null };
                    SetStatus(key, record);
                }
                return record;
            }
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
        if (_records.TryGetRecord<MedicalRecord>(patient, out var record) && status != TriageStatus.None)
        {
            SetStatus(patient, record with { Status = status, LastUpdated = _timing.CurTime });
        }
        else
        {
            SetStatus(patient, new MedicalRecord());
        }
    }

    public void ClaimPatient(StationRecordKey patient, EntityUid claimer)
    {
        var claimerName = Identity.Name(claimer, EntityManager);
        if (string.IsNullOrEmpty(claimerName))
            return;

        if (!_records.TryGetRecord<MedicalRecord>(patient, out var record))
            return;

        // Makes claim patient toggleable
        var newClaim = record.ClaimedName == claimerName ? null : claimerName;
        var newTime = newClaim != null ? (TimeSpan?)_timing.CurTime : null;

        SetStatus(patient, record with { ClaimedName = newClaim, LastUpdated = newTime });
    }
}
