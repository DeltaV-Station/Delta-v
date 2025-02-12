using Content.Shared._DV.Surgery;
using Content.Shared.Forensics;
using Content.Shared.FixedPoint;
using System.Linq;

namespace Content.Server._DV.Surgery;

/// <summary>
///     Responsible for handling the visual appearance of and sanitzation of items that can get dirty from surgery
/// </summary>
public sealed class SurgeryCleanSystem : SharedSurgeryCleanSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryCrossContaminationComponent, SurgeryCleanedEvent>(OnCleanDNA);
    }

    public override bool RequiresCleaning(EntityUid target)
    {
        var isDirty = (TryComp<SurgeryDirtinessComponent>(target, out var dirtiness) && dirtiness.Dirtiness > 0);
        var isContaminated = (TryComp<SurgeryCrossContaminationComponent>(target, out var contamination) && contamination.DNAs.Count > 0);

        return isDirty || isContaminated;
    }

    private void OnCleanDNA(Entity<SurgeryCrossContaminationComponent> ent, ref SurgeryCleanedEvent args)
    {
        var i = 0;
        var count = args.DnaAmount;
        ent.Comp.DNAs.RemoveWhere(item => i++ < count);
    }

    protected override void FinishCleaning(Entity<SurgeryCleansDirtComponent> ent, ref SurgeryCleanDirtDoAfterEvent args)
    {
        base.FinishCleaning(ent, ref args);

        // daisychain to forensics because if you sterilise something youve almost definitely scrubbed all dna and fibers off of it
        var daisyChainEvent = new CleanForensicsDoAfterEvent() { DoAfter = args.DoAfter };
        RaiseLocalEvent(ent.Owner, daisyChainEvent);
    }
}
