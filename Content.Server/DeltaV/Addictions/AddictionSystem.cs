using Content.Shared.DeltaV.Addictions;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeltaV.Addictions;

public sealed class AddictionSystem : SharedAddictionSystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _nextEffectTime = new();

    // Define the numbers, we're not making another DeepFryerSystem.cs
    // Minimum time between popups
    private const int MinEffectInterval = 10;

    // Maximum time between popups
    private const int MaxEffectInterval = 41;

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
            // If it's suppressed, check if it's still supposed to be
            if (component.Suppressed)
            {
                UpdateSuppressed(uid, component);
                continue;
            }

            if (!_nextEffectTime.TryGetValue(uid, out var nextTime))
            {
                // Between 10 and 40 seconds
                _nextEffectTime[uid] = curTime + TimeSpan.FromSeconds(_random.Next(MinEffectInterval, MaxEffectInterval));
                continue;
            }

            if (curTime < nextTime)
                continue;

            DoAddictionEffect(uid);
            _nextEffectTime[uid] = curTime + TimeSpan.FromSeconds(_random.Next(MinEffectInterval, MaxEffectInterval));
        }
    }

    private void OnShutdown(EntityUid uid, AddictedComponent component, ComponentShutdown args)
    {
        _nextEffectTime.Remove(uid);
    }

    private void UpdateSuppressed(EntityUid uid, AddictedComponent component)
    {
        var componentTime = component.LastMetabolismTime + TimeSpan.FromSeconds(10); // Ten seconds after the last metabolism cycle
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

    private string GetRandomPopup()
    {
        return _random.Pick(new[]
        {
            Loc.GetString("reagent-effect-medaddiction-1"),
            Loc.GetString("reagent-effect-medaddiction-2"),
            Loc.GetString("reagent-effect-medaddiction-3"),
            Loc.GetString("reagent-effect-medaddiction-4"),
            Loc.GetString("reagent-effect-medaddiction-5"),
            Loc.GetString("reagent-effect-medaddiction-6"),
            Loc.GetString("reagent-effect-medaddiction-7"),
            Loc.GetString("reagent-effect-medaddiction-8")
        });
    }

    public override void TryApplyAddiction(EntityUid uid, float addictionTime, StatusEffectsComponent? status = null)
    {
        base.TryApplyAddiction(uid, addictionTime, status);
    }

    protected override void DoAddictionEffect(EntityUid uid)
    {
        if (_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            _popupSystem.PopupEntity(GetRandomPopup(), uid, session);
        }
    }

    // Called each time a reagent with the Addicted effect gets metabolized
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
