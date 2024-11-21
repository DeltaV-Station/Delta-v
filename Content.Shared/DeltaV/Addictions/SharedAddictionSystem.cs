using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Addictions;

public abstract class SharedAddictionSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "Addicted";

    protected abstract void UpdateTime(EntityUid uid);

    public virtual void TryApplyAddiction(EntityUid uid, float addictionTime, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        UpdateTime(uid);

        if (!_statusEffects.HasStatusEffect(uid, StatusEffectKey, status))
        {
            _statusEffects.TryAddStatusEffect<AddictedComponent>(uid, StatusEffectKey, TimeSpan.FromSeconds(addictionTime), true, status);
        }
        else
        {
            _statusEffects.TryAddTime(uid, StatusEffectKey, TimeSpan.FromSeconds(addictionTime), status);
        }
    }

    public virtual void TrySuppressAddiction(EntityUid uid, float duration)
    {
        if (!TryComp<AddictedComponent>(uid, out var comp))
            return;

        var ent = new Entity<AddictedComponent>(uid, comp);
        UpdateAddictionSuppression(ent, duration);
    }

    protected virtual void UpdateAddictionSuppression(Entity<AddictedComponent> ent, float duration)
    {
    }
}
