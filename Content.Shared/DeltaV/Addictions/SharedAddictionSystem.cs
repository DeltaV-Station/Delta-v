
using Content.Shared.DeltaV.Addictions;
using Content.Shared.StatusEffect;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Addictions;

public abstract class SharedAddictionSystem : EntitySystem
{
    [Dependency] protected readonly StatusEffectsSystem StatusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "Addicted";

    public override void Initialize()
    {
        base.Initialize();
    }

    protected abstract void DoAddictionEffect(EntityUid uid, AddictedComponent component);

    protected abstract void UpdateTime(EntityUid uid);

    public virtual void TryApplyAddiction(EntityUid uid, float addictionStrength = 1f, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        UpdateTime(uid);

        if (!StatusEffects.HasStatusEffect(uid, StatusEffectKey, status))
        {
            StatusEffects.TryAddStatusEffect<AddictedComponent>(uid, StatusEffectKey, TimeSpan.FromSeconds(addictionStrength * 10), true, status);
        }
        else
        {
            StatusEffects.TryAddTime(uid, StatusEffectKey, TimeSpan.FromSeconds(addictionStrength * 10), status);
        }
    }
}
