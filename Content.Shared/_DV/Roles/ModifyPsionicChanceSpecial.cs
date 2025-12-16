using Content.Shared._DV.Psionics.Components;
using Content.Shared.Roles;

namespace Content.Shared._DV.Roles;

public sealed partial class ModifyPsionicChanceSpecial : JobSpecial
{
    /// <summary>
    /// The value to replace the JobChance with.
    /// </summary>
    [DataField(required: true)]
    public float JobBonusChance;

    /// <summary>
    /// If not null, it'll replace species bonus too.
    /// </summary>
    [DataField]
    public float? SpeciesBonusChance;


    public override void AfterEquip(EntityUid mob)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        if (!entityManager.TryGetComponent(mob, out PotentialPsionicComponent? psionic))
            return;

        psionic.JobBonusChance = JobBonusChance;
        if (SpeciesBonusChance.HasValue)
            psionic.SpeciesBonusChance = SpeciesBonusChance.Value;
    }
}
