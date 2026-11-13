using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._DV.Weather;

/// <summary>
/// Handles weather damage for exposed entities.
/// </summary>
public sealed partial class WeatherEffectsSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    private EntityQuery<MapGridComponent> _gridQuery;

    /// <summary>
    /// How long to wait between updating weather effects.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<WeatherSchedulerComponent>();
        while (query.MoveNext(out var map, out var weatherSchedulerComponent))
        {
            if (now < weatherSchedulerComponent.NextDamageUpdate)
                continue;

            weatherSchedulerComponent.NextDamageUpdate = now + UpdateDelay;

            var currentStage = weatherSchedulerComponent.Stage > 0 ? weatherSchedulerComponent.Stage - 1 : 0;
            var weatherStage = weatherSchedulerComponent.Stages[currentStage];

            UpdateDamage(map, weatherStage);
        }
    }

    private void UpdateDamage(EntityUid map, WeatherStage? weather)
    {
        if (weather == null || weather.Value.Damage is not {} damage)
            return;

        var query = EntityQueryEnumerator<MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var mob, out var xform))
        {
            // don't give dead bodies 10000 burn, that's not fun for anyone
            if (xform.MapUid != map || mob.CurrentState == MobState.Dead)
                continue;

            // if not in space, check for being indoors
            if (xform.GridUid is {} gridUid && _gridQuery.TryComp(gridUid, out var grid))
            {
                var tile = _map.GetTileRef((gridUid, grid), xform.Coordinates);
                if (!_weather.CanWeatherAffect((gridUid, grid), tile))
                    continue;
            }

            _damageable.TryChangeDamage(uid, damage, interruptsDoAfters: false);
        }
    }
}
