using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Pain;

public abstract class SharedPainSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "InPain";

    protected abstract void UpdatePainSuppression(Entity<PainComponent> ent, float duration, PainSuppressionLevel level);

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

    public virtual void TrySuppressPain(EntityUid uid, float duration, PainSuppressionLevel level = PainSuppressionLevel.Normal)
    {
        if (!TryComp<PainComponent>(uid, out var comp))
            return;

        UpdatePainSuppression((uid, comp), duration, level);
    }
}

// Used by the StatusEffect
public enum PainSuppressionLevel : byte
{
    Mild,
    Normal,
    Strong,
}
