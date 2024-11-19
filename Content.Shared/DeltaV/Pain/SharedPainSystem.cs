using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Pain;

public abstract class SharedPainSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "InPain";

    protected abstract void UpdatePainSuppression(Entity<PainComponent> ent, float duration, byte level);

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

    public virtual void TrySuppressPain(EntityUid uid, float duration, byte level = PainSuppressionLevelExtensions.Normal)
    {
        if (!TryComp<PainComponent>(uid, out var comp))
            return;

        UpdatePainSuppression((uid, comp), duration, level);
    }
}

// Used by the StatusEffect
public static class PainSuppressionLevelExtensions
{
    public const byte Mild = 0;
    public const byte Normal = 1;
    public const byte Strong = 2;

    public static string ToDisplayString(this byte level)
    {
        return level switch
        {
            Mild => nameof(Mild),
            Normal => nameof(Normal),
            Strong => nameof(Strong),
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    public static bool IsValid(byte level)
    {
        return level <= Strong;
    }
}
