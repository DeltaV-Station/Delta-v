using Content.Server.Body.Components;
using Content.Server.Forensics;
using Content.Server.Objectives.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DeltaV.Recruiter;
using Content.Shared.Popups;

namespace Content.Server.DeltaV.Recruiter;

/// <summary>
/// Handles bloodstream related code since that isn't in shared.
/// </summary>
public sealed class RecruiterPenSystem : SharedRecruiterPenSystem
{
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly SolutionTransferSystem _transfer = default!;

    protected override void DrawBlood(EntityUid uid, Entity<SolutionComponent> dest, EntityUid user)
    {
        // how did you even use this mr plushie...
        if (CompOrNull<BloodstreamComponent>(user)?.BloodSolution is not {} blood)
            return;

        var desired = dest.Comp.Solution.AvailableVolume;
        if (_transfer.Transfer(user, user, blood, uid, dest, desired) != desired)
            return;

        // this is why you have to keep the pen safe, it has the dna of everyone you recruited!
        _forensics.TransferDna(uid, user, canDnaBeCleaned: false);

        Popup.PopupEntity(Loc.GetString("recruiter-pen-pricked", ("pen", Name(uid))), user, user, PopupType.LargeCaution);
    }

    protected override bool Recruit(Entity<RecruiterPenComponent> ent, EntityUid user)
    {
        if (!base.Recruit(ent, user))
            return false;

        if (Mind.TryGetObjectiveComp<RecruitingConditionComponent>(user, out var obj))
            obj.Recruited++;

        return true;
    }
}
