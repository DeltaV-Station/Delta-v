using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;

namespace Content.Shared._DV.MedicalRecords;

public abstract class SharedMedicalRecordsSystem : EntitySystem
{
    public void UpdateMedicalRecords(string name, MedicalRecord status)
    {
        var query = EntityQueryEnumerator<IdentityComponent>();

        while (query.MoveNext(out var uid, out var identity))
        {
            if (!Identity.Name(uid, EntityManager).Equals(name))
                continue;

            if (status.Status == TriageStatus.None)
            {
                RemComp<MedicalRecordComponent>(uid);
            }
            else
            {
                EnsureComp<MedicalRecordComponent>(uid, out var record);
                record.Record = status;
                Dirty(uid, record);
            }
        }
    }
}
