using Content.Client.Items;
using Content.Shared._DV.MedicalRecords;

namespace Content.Client._DV.MedicalRecords.EntitySystems;

public sealed class TriageRemoteSystem : SharedTriageRemoteSystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<TriageRemoteComponent>(ent => new TriageRemoteStatusControl(ent));
    }
}
