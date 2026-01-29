using Content.Server._DV.StationEvents.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Light.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
namespace Content.Server._DV.StationEvents.Events;

/// <summary>
/// This event flickers a portion of the lights on the station while the game rule is running.
/// </summary>
public sealed class FlickeringLightsRule : StationEventSystem<FlickeringLightsRuleComponent>
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PoweredLightSystem _lightSystem = default!;

    private EntityQuery<PoweredLightComponent> _lightQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lightQuery = GetEntityQuery<PoweredLightComponent>();
    }

    protected override void Started(EntityUid uid, FlickeringLightsRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Whatever starts the rule may have set the station before starting the game rule
        // If that's the case, we shouldn't randomly choose a station.
        if (!component.AffectedStation.HasValue)
        {
            if (!TryGetRandomStation(out var chosenStation))
            {
                Log.Error($"{ToPrettyString(uid):rule} failed to find a station!");
                return;
            }

            component.AffectedStation = chosenStation.Value;
        }

        var grid = StationSystem.GetLargestGrid(component.AffectedStation.Value);

        if (!grid.HasValue)
        {
            Log.Error($"{ToPrettyString(uid):rule} picked station {component.AffectedStation.Value} which had no largest grid!");
            ForceEndSelf(uid, gameRule);
            return;
        }

        var mapId = Transform(grid.Value).MapID;

        var lightLookup = new HashSet<Entity<PoweredLightComponent>>();
        _entityLookup.GetEntitiesOnMap<PoweredLightComponent>(mapId, lightLookup);

        foreach (var light in lightLookup)
        {
            // We do not want to affect lights that are already blinking
            // It might be caused by ghosts or the variation pass, or otherwise is not controlled by us.
            if (light.Comp.IsBlinking)
                continue;

            if(Transform(light).MapID != mapId)
                continue;

            if (!_random.Prob(component.LightFlickerChance))
                continue;


            component.AffectedEntities.Add(light.Owner, !light.Comp.IgnoreGhostsBoo);

            _lightSystem.ToggleBlinkingLight(light, light, true);

            // Prevent ghosts from playing with the lights during this time.
            if(!light.Comp.IgnoreGhostsBoo)
                _lightSystem.ToggleIgnoreGhostBoo(light, light, true);
        }
    }

    protected override void Ended(EntityUid uid, FlickeringLightsRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        foreach (var (light, shouldResetIgnore) in component.AffectedEntities)
        {
            if(!_lightQuery.TryComp(light, out var lightComp))
                continue;

            _lightSystem.ToggleBlinkingLight(light, lightComp, false);

            if(shouldResetIgnore)
                _lightSystem.ToggleIgnoreGhostBoo(light, lightComp, false);
        }

        component.AffectedEntities.Clear();
    }
}
