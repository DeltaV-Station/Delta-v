using Content.Shared.DeltaV.Pain;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Pain;

public sealed class PainSystem : SharedPainSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PainComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<PainComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _timing.CurTime;
        ent.Comp.NextPopupTime = _timing.CurTime;
    }

    protected override void UpdatePainSuppression(Entity<PainComponent> ent, float duration, PainSuppressionLevel level)
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

    private void UpdateSuppressed(Entity<PainComponent> ent)
    {
        ent.Comp.Suppressed = (_timing.CurTime < ent.Comp.SuppressionEndTime);
        Dirty(ent);
    }

    private void ShowPainPopup(Entity<PainComponent> ent)
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
        var query = EntityQueryEnumerator<PainComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdateTime)
                continue;

            var ent = new Entity<PainComponent>(uid, component);

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
