using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Addictions;

public abstract class SharedAddictionSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public EntProtoId AddictedStatusEffectProto = "Addicted";

    protected abstract void UpdateTime(EntityUid uid);

    public virtual void TryApplyAddiction(Entity<AddictionComponent?> ent, float addictionTime)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        UpdateTime(ent.Owner);

        if (!_statusEffects.HasStatusEffect(ent.Owner, StatusEffectKey, ent.Comp))
        {
            _statusEffects.TryAddStatusEffect<AddictedComponent>(ent.Owner, StatusEffectKey, TimeSpan.FromSeconds(addictionTime), true, ent.Comp);
        }
        else
        {
            _statusEffects.TryAddTime(ent.Owner, StatusEffectKey, TimeSpan.FromSeconds(addictionTime), ent.Comp);
        }
    }

    public virtual void TrySuppressAddiction(Entity<AddictedComponent?> ent, float duration)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        UpdateAddictionSuppression((ent.Owner, ent.Comp), duration);
    }

    protected virtual void UpdateAddictionSuppression(Entity<AddictedComponent> ent, float duration)
    {
    }
}
