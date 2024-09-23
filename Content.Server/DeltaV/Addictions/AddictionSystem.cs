using Content.Shared.DeltaV.Addictions;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Addictions;

public sealed class AddictionSystem : SharedAddictionSystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _nextEffectTime = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AddictedComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<AddictedComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Suppressed)
            {
                UpdateSuppressed(uid, component);
                continue;
            }

            if (!_nextEffectTime.TryGetValue(uid, out var nextTime))
            {
                // _nextEffectTime[uid] = curTime + TimeSpan.FromSeconds(Random.NextDouble() * 9 + 1);
                _nextEffectTime[uid] = curTime + TimeSpan.FromSeconds(5);
                continue;
            }

            if (curTime < nextTime)
                continue;

            DoAddictionEffect(uid, component);
            // _nextEffectTime[uid] = curTime + TimeSpan.FromSeconds(Random.NextDouble() * 9 + 1);
            _nextEffectTime[uid] = curTime + TimeSpan.FromSeconds(5);
        }
    }

    private void OnShutdown(EntityUid uid, AddictedComponent component, ComponentShutdown args)
    {
        _nextEffectTime.Remove(uid);
    }

    private void UpdateSuppressed(EntityUid uid, AddictedComponent component)
    {
        var componentTime = component.LastMetabolismTime + TimeSpan.FromSeconds(10);
        if (componentTime > _timing.CurTime)
        {
            component.Suppressed = true;
            Dirty(uid, component);
        }
        else
        {
            component.Suppressed = false;
            Dirty(uid, component);
        }
    }
    protected override void DoAddictionEffect(EntityUid uid, AddictedComponent component)
    {
        var message = Loc.GetString("addiction-effect", ("entity", uid));
        _popupSystem.PopupEntity(message, uid);
    }

    public override void TryApplyAddiction(EntityUid uid, float addictionStrength = 1f, StatusEffectsComponent? status = null)
    {
        base.TryApplyAddiction(uid, addictionStrength, status);
    }

    protected override void UpdateTime(EntityUid uid)
    {
        if (TryComp<AddictedComponent>((uid), out var component))
        {
            component.LastMetabolismTime = _timing.CurTime;
            UpdateSuppressed(uid, component);
        }
        else
            return;
    }
}
