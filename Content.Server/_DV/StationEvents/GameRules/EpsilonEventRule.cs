using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Content.Server.AlertLevel;
using Content.Server.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.StationEvents.Events
{
public sealed class EpsilonEventRule : StationEventSystem<EpsilonEventRuleComponent>
{
    [Dependency] private readonly ApcSystem _apcSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected override void Started(EntityUid uid, EpsilonEventRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // This should be checked before playing the sound
        if (!TryGetRandomStation(out var chosenStation))
            return;
        component.AffectedStation = chosenStation.Value;

        // Plays the power off sound to the station.
        _sound.PlayGlobalOnStation(component.AffectedStation, _audio.ResolveSound(component.PowerOffSound));

        var query = AllEntityQuery<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var transform))
        {

            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station != chosenStation)
                continue;

            // Turns off APCs during the event. Basic power-outage event.
            if (apc.MainBreakerEnabled)
            {
                _apcSystem.ApcToggleBreaker(apcUid, apc);
                component.ToggledAPCs.Add((apcUid, apc));
            }
        }
    }

    protected override void Ended(EntityUid uid, EpsilonEventRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        // Checks if station exists.
        if (!Exists(component.AffectedStation))
            return;

        // Restores the power to the station.
        foreach (var apc in component.ToggledAPCs)
        {
            if (Deleted(apc))
                continue;

            if (!apc.Comp.MainBreakerEnabled)
                _apcSystem.ApcToggleBreaker(apc, apc);
        }
        component.ToggledAPCs.Clear();

        // Final effect, sets the code to Epsilon
        if (!HasComp<AlertLevelComponent>(component.AffectedStation))
            return;

        _alertLevelSystem.SetLevel(component.AffectedStation, "epsilon", true, true, true);
    }
}
}
