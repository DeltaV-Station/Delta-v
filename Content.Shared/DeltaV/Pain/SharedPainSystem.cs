using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.DeltaV.Pain;

public abstract class SharedPainSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "InPain";

    protected abstract void UpdatePainSuppression(EntityUid uid, float duration);

    public virtual void TryApplyPain(EntityUid uid, float painTime, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (!_statusEffects.HasStatusEffect(uid, StatusEffectKey, status))
        {
            _statusEffects.TryAddStatusEffect<PainComponent>(uid, StatusEffectKey, TimeSpan.FromSeconds(painTime), true, status);
        }
        else
        {
            _statusEffects.TryAddTime(uid, StatusEffectKey, TimeSpan.FromSeconds(painTime), status);
        }
    }

    public virtual void TrySuppressPain(EntityUid uid, float duration)
    {
        if (!TryComp<PainComponent>(uid, out _))
            return;

        UpdatePainSuppression(uid, duration);
    }
}
