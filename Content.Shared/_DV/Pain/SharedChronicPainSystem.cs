using Content.Shared.StatusEffect;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._DV.ChronicPain;

public abstract class SharedChronicPainSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public ProtoId<StatusEffectPrototype> StatusEffectKey = "ChronicPain";

    public override void Initialize()
    {
        SubscribeLocalEvent<ChronicPainComponent, MapInitEvent>(OnMapInit);
    }

    public void TryApplyPain(EntityUid uid, float painTime, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (!_statusEffects.HasStatusEffect(uid, StatusEffectKey, status))
        {
            _statusEffects.TryAddStatusEffect<ChronicPainComponent>(uid, StatusEffectKey, TimeSpan.FromSeconds(painTime), true, status);
        }
        else
        {
            _statusEffects.TryAddTime(uid, StatusEffectKey, TimeSpan.FromSeconds(painTime), status);
        }
    }

    public void TrySuppressPain(EntityUid uid, float duration, PainSuppressionLevel level = PainSuppressionLevel.Normal)
    {
        if (!TryComp<ChronicPainComponent>(uid, out var comp))
            return;

        UpdatePainSuppression((uid, comp), duration, level);
    }

    protected void UpdatePainSuppression(Entity<ChronicPainComponent> ent, float duration, PainSuppressionLevel level)
    {
        var curTime = _timing.CurTime;
        var newEndTime = curTime + TimeSpan.FromSeconds(duration);

        // Only update if this would extend the suppression
        if (newEndTime <= ent.Comp.SuppressionEndTime)
            return;

        ent.Comp.LastPainkillerTime = curTime;
        ent.Comp.SuppressionEndTime = newEndTime;
        UpdateSuppressed(ent);
    }

    private void UpdateSuppressed(Entity<ChronicPainComponent> ent)
    {
        ent.Comp.Suppressed = _timing.CurTime < ent.Comp.SuppressionEndTime;
        Dirty(ent);
    }


    private void OnMapInit(Entity<ChronicPainComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _timing.CurTime;
        ent.Comp.NextPopupTime = _timing.CurTime;
    }

    private void ShowPainPopup(Entity<ChronicPainComponent> ent)
    {
        if (!_prototype.TryIndex(ent.Comp.DatasetPrototype, out var dataset))
            return;

        var effects = dataset.Values;
        if (effects.Count == 0)
            return;

        var effect = _random.Pick(effects);
        _popup.PopupEntity(Loc.GetString(effect), ent, ent);

        // Set next popup time
        var delay = _random.NextFloat(ent.Comp.MinimumPopupDelay, ent.Comp.MaximumPopupDelay);
        ent.Comp.NextPopupTime = _timing.CurTime + TimeSpan.FromSeconds(delay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ChronicPainComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdateTime)
                continue;

            var ent = new Entity<ChronicPainComponent>(uid, component);

            if (component.Suppressed)
            {
                UpdateSuppressed(ent);
            }
            else if (curTime >= component.NextPopupTime)
            {
                ShowPainPopup(ent);
            }
            component.NextUpdateTime = curTime + TimeSpan.FromSeconds(1);
        }
    }
}

// Used by the StatusEffect
public enum PainSuppressionLevel : byte
{
    Mild,
    Normal,
    Strong,
}
