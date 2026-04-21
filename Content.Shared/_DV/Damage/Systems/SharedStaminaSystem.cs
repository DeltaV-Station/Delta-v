using Content.Shared._DV.Psionics.Events;
using Content.Shared.Damage.Components;

namespace Content.Shared.Damage.Systems;

/// <summary>
/// This is for DV specific alterations, so we don't merge conflict upstream stuff.
/// </summary>
public abstract partial class SharedStaminaSystem
{
    private void InitializeDVStamina()
    {
        SubscribeLocalEvent<StaminaComponent, IndomitableWillSuccessEvent>(OnIndomitableSuccess);
    }

    private void OnIndomitableSuccess(Entity<StaminaComponent> entity, ref IndomitableWillSuccessEvent args)
    {
        if (entity.Comp.StaminaDamage >= entity.Comp.CritThreshold)
        {
            ExitStamCrit(entity, entity.Comp);
        }

        entity.Comp.StaminaDamage = 0;
        AdjustStatus(entity.Owner);
        RemComp<ActiveStaminaComponent>(entity);
        _status.TryRemoveStatusEffect(entity, StaminaLow);
        UpdateStaminaVisuals(entity);
        Dirty(entity);
    }
}
