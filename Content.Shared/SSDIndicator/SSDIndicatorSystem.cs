using Content.Shared._DV.CCVars; // DeltaV - SSD Recency
using Content.Shared._DV.Mind; // DeltaV - SSD Recency
using Content.Shared.CCVar;
using Content.Shared.Examine; // DeltaV - SSD Recency
using Content.Shared.Mobs.Systems; // DeltaV - SSD Recency
using Content.Shared.Mind.Components; // DeltaV - SSD Recency
using Content.Shared.StatusEffectNew;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.SSDIndicator;

/// <summary>
///     Handle changing player SSD indicator status
/// </summary>
public sealed class SSDIndicatorSystem : EntitySystem
{
    public static readonly EntProtoId StatusEffectSSDSleeping = "StatusEffectSSDSleeping";

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!; // DeltaV - SSD Recency

    private bool _icSsdSleep;
    private float _icSsdSleepTime;

    private TimeSpan _cryoableSsdSeconds; // DeltaV - SSD Recency
    private TimeSpan _recentSsdSeconds; // DeltaV - SSD Recency

    public override void Initialize()
    {
        SubscribeLocalEvent<SSDIndicatorComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SSDIndicatorComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<SSDIndicatorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SSDIndicatorComponent, MindStateUpdatedEvent>(OnMindStateUpdated); // DeltaV - SSD Recency

        _cfg.OnValueChanged(CCVars.ICSSDSleep, obj => _icSsdSleep = obj, true);
        _cfg.OnValueChanged(CCVars.ICSSDSleepTime, obj => _icSsdSleepTime = obj, true);
        _cfg.OnValueChanged(DCCVars.SsdIndicatorCryoableAfterSeconds, obj => _cryoableSsdSeconds = TimeSpan.FromSeconds(obj), true); // DeltaV - SSD Recency
        _cfg.OnValueChanged(DCCVars.SsdIndicatorRecentAfterSeconds, obj => _recentSsdSeconds = TimeSpan.FromSeconds(obj), true); // DeltaV - SSD Recency

        SubscribeLocalEvent<SSDIndicatorComponent, ExaminedEvent>(OnExamine); // DeltaV - SSD Recency
    }

    // DeltaV - SSD Recency START
    private void OnExamine(Entity<SSDIndicatorComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.SsdSince is not { } ssdSince)
            return;

        if (_mobState.IsDead(ent))
            return;

        using (args.PushGroup(nameof(SSDIndicatorComponent)))
        {
            var timestamp = (_timing.CurTime - ssdSince).ToString("%hh':'mm':'ss");
            args.PushMarkup(Loc.GetString("ssd-examine-duration", ("time", timestamp)));
            args.PushMarkup(Loc.GetString($"ssd-examine-{GetStage(ent).ToString().ToLower()}"));
        }
    }

    public SsdStage GetStage(Entity<SSDIndicatorComponent> ent)
    {
        var curTime = _timing.CurTime;

        if (ent.Comp.SsdSince + _recentSsdSeconds >= curTime)
        {
            return SsdStage.VeryRecent;
        }

        if (ent.Comp.SsdSince + _cryoableSsdSeconds >= curTime)
        {
            return SsdStage.Recent;
        }

        return SsdStage.Cryoable;
    }

    public void OnMindStateUpdated(Entity<SSDIndicatorComponent> ent, ref MindStateUpdatedEvent args)
    {
        if (args.State is MindState.SSD or MindState.DeadSSD) {
            if (ent.Comp.SsdSince is null)
                 ent.Comp.SsdSince = _timing.CurTime;
        }
        else
            ent.Comp.SsdSince = null;
        Dirty(ent, ent.Comp);
    }
    // DeltaV END

    private void OnPlayerAttached(EntityUid uid, SSDIndicatorComponent component, PlayerAttachedEvent args)
    {
        component.IsSSD = false;

        // Removes force sleep and resets the time to zero
        if (_icSsdSleep)
        {
            component.FallAsleepTime = TimeSpan.Zero;
            _statusEffects.TryRemoveStatusEffect(uid, StatusEffectSSDSleeping);
        }

        Dirty(uid, component);
    }

    private void OnPlayerDetached(EntityUid uid, SSDIndicatorComponent component, PlayerDetachedEvent args)
    {
        component.IsSSD = true;

        // Sets the time when the entity should fall asleep
        if (_icSsdSleep)
        {
            component.FallAsleepTime = _timing.CurTime + TimeSpan.FromSeconds(_icSsdSleepTime);
        }

        Dirty(uid, component);
    }

    // Prevents mapped mobs to go to sleep immediately
    private void OnMapInit(EntityUid uid, SSDIndicatorComponent component, MapInitEvent args)
    {
        if (!_icSsdSleep || !component.IsSSD)
            return;

        component.FallAsleepTime = _timing.CurTime + TimeSpan.FromSeconds(_icSsdSleepTime);
        component.NextUpdate = _timing.CurTime + component.UpdateInterval;
        Dirty(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_icSsdSleep)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SSDIndicatorComponent>();

        while (query.MoveNext(out var uid, out var ssd))
        {
            // Forces the entity to sleep when the time has come
            if (!ssd.IsSSD
                || ssd.NextUpdate > curTime
                || ssd.FallAsleepTime > curTime
                || TerminatingOrDeleted(uid))
                continue;

            _statusEffects.TryUpdateStatusEffectDuration(uid, StatusEffectSSDSleeping);
            ssd.NextUpdate += ssd.UpdateInterval;
            Dirty(uid, ssd);
        }
    }
}
