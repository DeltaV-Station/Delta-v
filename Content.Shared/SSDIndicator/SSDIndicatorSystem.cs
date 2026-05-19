using Content.Shared._DV.CCVars; // DeltaV - SSD Recency
using Content.Shared.CCVar;
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

    private bool _icSsdSleep;
    private float _icSsdSleepTime;

    private float _cryoableSsdSeconds; // DeltaV
    private float _recentSsdSeconds; // DeltaV

    public override void Initialize()
    {
        SubscribeLocalEvent<SSDIndicatorComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SSDIndicatorComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<SSDIndicatorComponent, MapInitEvent>(OnMapInit);

        _cfg.OnValueChanged(CCVars.ICSSDSleep, obj => _icSsdSleep = obj, true);
        _cfg.OnValueChanged(CCVars.ICSSDSleepTime, obj => _icSsdSleepTime = obj, true);
        _cfg.OnValueChanged(DCCVars.SsdIndicatorCryoableAfterSeconds, OnCryoableDurationChanged, true); // DeltaV - Recency
        _cfg.OnValueChanged(DCCVars.SsdIndicatorRecentAfterSeconds, OnRecentDurationChanged, true); // DeltaV - Recency
    }

    // DeltaV - SSD Recency START
    private void OnRecentDurationChanged(float obj)
    {
        _recentSsdSeconds = obj;

        var query = EntityQueryEnumerator<SSDIndicatorComponent>();
        while (query.MoveNext(out var entity, out var ssd))
        {
            if (!ssd.IsSSD || TerminatingOrDeleted(entity))
                continue;

            ssd.RecentSsdTime = ssd.SsdSince + TimeSpan.FromSeconds(_recentSsdSeconds);
            ssd.Stage = SsdStage.VeryRecent;

            Dirty(entity, ssd);
        }
    }

    private void OnCryoableDurationChanged(float obj)
    {
        _cryoableSsdSeconds = obj;

        var query = EntityQueryEnumerator<SSDIndicatorComponent>();
        while (query.MoveNext(out var entity, out var ssd))
        {
            if (!ssd.IsSSD || TerminatingOrDeleted(entity))
                continue;

            ssd.CryoableSsdTime = ssd.SsdSince + TimeSpan.FromSeconds(_cryoableSsdSeconds);
            ssd.Stage = SsdStage.VeryRecent;

            Dirty(entity, ssd);
        }
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

        // DeltaV - Recency START
        component.SsdSince = _timing.CurTime;
        component.RecentSsdTime = _timing.CurTime + TimeSpan.FromSeconds(_recentSsdSeconds);
        component.CryoableSsdTime = _timing.CurTime + TimeSpan.FromSeconds(_cryoableSsdSeconds);
        // DeltaV END

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

        // DeltaV - Don't return, we're checking SSD duration below. Moved down.
        // if (!_icSsdSleep)
        //    return;
        // DeltaV END

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SSDIndicatorComponent>();

        while (query.MoveNext(out var uid, out var ssd))
        {
            // Forces the entity to sleep when the time has come
            if (!ssd.IsSSD
                || ssd.NextUpdate > curTime
                // || ssd.FallAsleepTime > curTime // DeltaV - moved down
                || TerminatingOrDeleted(uid))
                continue;

            // DeltaV - Recency START
            if (ssd.Stage != SsdStage.Cryoable) // Avoid unnecessary dirtying once last stage reached.
            {
                if (ssd.CryoableSsdTime < curTime)
                {
                    ssd.Stage = SsdStage.Cryoable;
                }
                else if (ssd.RecentSsdTime < curTime)
                {
                    ssd.Stage = SsdStage.Recent;
                }
                else
                {
                    ssd.Stage = SsdStage.VeryRecent;
                }
                Dirty(uid, ssd);
            }

            if (!_icSsdSleep || ssd.FallAsleepTime > curTime)
                continue;
            // DeltaV END

            _statusEffects.TryUpdateStatusEffectDuration(uid, StatusEffectSSDSleeping);
            ssd.NextUpdate += ssd.UpdateInterval;
            Dirty(uid, ssd);
        }
    }
}
