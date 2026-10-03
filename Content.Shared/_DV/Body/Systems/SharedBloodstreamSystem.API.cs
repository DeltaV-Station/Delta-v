using Content.Shared.Body.Components;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBloodstreamSystem
{
    [PublicAPI]
    public void ModifyBloodRefreshAmount(Entity<BloodstreamComponent?> ent, FixedPoint2 newAmount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.BloodRefreshAmount = newAmount;
        Dirty(ent);
    }

    [PublicAPI]
    public void ModifyBloodlossHealAmount(Entity<BloodstreamComponent?> ent, FixedPoint2 modifier)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.BloodlossHealDamage *= modifier;
        Dirty(ent);
    }
}
