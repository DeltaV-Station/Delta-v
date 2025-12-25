using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Body.Systems;

public sealed partial class SharedResurrectWhenAbleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResurrectWhenAbleComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ResurrectWhenAbleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Don't bother processing alive mobs
            if (!_mobState.IsDead(uid))
            {
                comp.ResurrectAt = null;
                continue;
            }

            if (!_mobThreshold.TryGetThresholdForState(uid, Mobs.MobState.Dead, out var threshold))
                continue;

            if (!TryComp<DamageableComponent>(uid, out var damageable))
                continue;

            // We're dead, and must stay dead.
            if (damageable.TotalDamage >= threshold)
            {
                comp.ResurrectAt = null;
                continue;
            }

            // We can resurrect, but aren't currently counting down.
            if (comp.ResurrectAt is not { } resurrectTime)
            {
                comp.ResurrectAt = _timing.CurTime + TimeSpan.FromSeconds(comp.TimeToResurrect);
                continue;
            }

            // We can resurrect, and are counting down, but haven't reached the time yet.
            if (_timing.CurTime < resurrectTime)
                continue;

            // Resurrect the entity.
            _mobState.ChangeMobState(uid, Mobs.MobState.Alive);
            comp.ResurrectAt = null;
        }
    }

    private void OnExamined(Entity<ResurrectWhenAbleComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        // Only show the resurrection text if we're actually going to resurrect.
        if (entity.Comp.ResurrectAt is not { })
            return;

        if (entity.Comp.ResurrectDesc is not { } desc)
            return;

        args.PushMarkup(Loc.GetString(desc));
    }
}
