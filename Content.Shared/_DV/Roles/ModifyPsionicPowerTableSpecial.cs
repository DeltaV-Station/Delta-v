using Content.Shared._DV.Psionics.Components;
using Content.Shared.EntityTable;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Roles;

public sealed partial class ModifyPsionicPowerTableSpecial : JobSpecial
{
    /// <summary>
    /// The Prototype ID of the table containing the available psionic powers to roll.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<EntityTablePrototype> PsionicPowerTableId;

    public override void AfterEquip(EntityUid mob)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        if (!entityManager.TryGetComponent(mob, out PotentialPsionicComponent? psionic))
            return;

        psionic.PsionicPowerTableId = PsionicPowerTableId;
    }
}
