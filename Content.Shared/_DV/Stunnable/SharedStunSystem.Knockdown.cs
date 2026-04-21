using Content.Shared._DV.Psionics.Events;

namespace Content.Shared.Stunnable;

/// <summary>
/// This is for DV specific alterations, so we don't merge conflict upstream stuff.
/// </summary>
public abstract partial class SharedStunSystem
{
    private void InitializeDVKnockdown()
    {
        SubscribeLocalEvent<KnockedDownComponent, IndomitableWillSuccessEvent>(OnRejuvenate);
    }

    private void OnRejuvenate(Entity<KnockedDownComponent> entity, ref IndomitableWillSuccessEvent ev)
    {
        SetKnockdownNextUpdate(entity, GameTiming.CurTime);

        if (entity.Comp.AutoStand)
            RemComp<KnockedDownComponent>(entity);
    }
}
