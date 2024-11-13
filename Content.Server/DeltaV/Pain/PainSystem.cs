using Content.Shared.DeltaV.Pain;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Pain;

public sealed class PainSystem : SharedPainSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PainComponent, ComponentStartup>(OnInit);
    }

    private void OnInit(Entity<PainComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextUpdateTime = _timing.CurTime;
    }

    protected override void UpdatePainSuppression(EntityUid uid, float duration)
    {
        if (!TryComp<PainComponent>(uid, out var component))
            return;

        var ent = new Entity<PainComponent>(uid, component);
        var curTime = _timing.CurTime;
        var newEndTime = curTime + TimeSpan.FromSeconds(duration);

        // Only update if this would extend the suppression
        if (newEndTime <= component.SuppressionEndTime)
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

            component.NextUpdateTime = curTime + TimeSpan.FromSeconds(1);
        }
    }
}
