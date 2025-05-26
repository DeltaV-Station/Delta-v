using Content.Shared._DV.MedicalRecords;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;

namespace Content.Server._DV.MedicalRecords;

public sealed class TriageRemoteSystem : SharedTriageRemoteSystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly MedicalRecordsSystem _records = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriageRemoteComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnBeforeInteract(Entity<TriageRemoteComponent> entity, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled
            || args.Target == null
            || !HasComp<MobStateComponent>(args.Target)
            || !_examine.InRangeUnOccluded(args.User, args.Target.Value, 25f))  // TODO Check for range, not occlusion (maybe)
        {
            return;
        }

        args.Handled = true;

        var records = _records.GetMedicalRecords(args.Target.Value);
        switch (entity.Comp.Mode)
        {
            case OperatingMode.GiveDnr:
                SetOrClearMode(args.Target.Value, records, TriageStatus.Dnr);
                break;
            case OperatingMode.GiveLow:
                SetOrClearMode(args.Target.Value, records, TriageStatus.Low);
                break;
            case OperatingMode.GiveHigh:
                SetOrClearMode(args.Target.Value, records, TriageStatus.High);
                break;
            default:
                throw new InvalidOperationException("Invalid triage mode");

        }
        Dirty(entity);
    }

    private void SetOrClearMode(EntityUid target, MedicalRecord currentRecord, TriageStatus newStatus)
    {
        if (currentRecord.Status == newStatus && currentRecord.IsCommandLevelTriage)
        {
            // Clear triage status
            _records.SetPatientStatus(target, TriageStatus.Normal, false);
            return;
        }
        _records.SetPatientStatus(target, newStatus, true);
    }
}
