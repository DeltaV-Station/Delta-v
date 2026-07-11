using Content.Server.AlertLevel;
using Content.Server.Audio.Jukebox;
using Content.Server.Instruments;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Nuke;
using Content.Server.RoundEnd;
using Content.Shared._DV.KeycardAuthenticationDevice;
using Content.Shared.Audio.Jukebox;
using Content.Shared.Light.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.KeycardAuthenticationDevice;

public sealed class DVStationKeycardAuthenticationDeviceSystem : SharedDVStationKeycardAuthenticationDeviceSystem
{
    [Dependency] private readonly NukeCodePaperSystem _nukeCodePaper = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly JukeboxSystem _jukebox = default!;

    protected override void Mayday(Entity<DVStationKeycardAuthenticationDeviceComponent> station)
    {
        base.Mayday(station);

        _alertLevel.SetLevel(station, "zeta", true, true, true, true);
        var alertLevel = Comp<AlertLevelComponent>(station);
        var level = _prototype.Index<AlertLevelPrototype>(alertLevel.AlertLevelPrototype).Levels[alertLevel.CurrentLevel];
        _roundEnd.RequestRoundEnd(level.ShuttleTime, null, null, false, cantRecall: true);

        var bulbQuery = GetEntityQuery<LightBulbComponent>();
        var tubeQuery = EntityQueryEnumerator<PoweredLightComponent>();
        while (tubeQuery.MoveNext(out var uid, out var light))
        {
            if (Station.GetOwningStation(uid) != station.Owner)
                continue;

            if (_poweredLight.GetBulb(uid, light) is not { } bulb || !bulbQuery.TryComp(bulb, out var bulbComp))
                continue;

            bulbComp.LightEnergy /= 2;
            bulbComp.PowerUse /= 2;

            _poweredLight.SetState(uid, light.On, light);
        }

        var emergencyQuery = EntityQueryEnumerator<EmergencyLightComponent, PointLightComponent>();
        while (emergencyQuery.MoveNext(out var uid, out var emergency, out var light))
        {
            if (Station.GetOwningStation(uid) != station.Owner)
                continue;

            _pointLight.SetEnergy(uid, emergency.MaydayEnergy, light);
            _pointLight.SetRadius(uid, emergency.MaydayRadius, light);
            _pointLight.SetFalloff(uid, emergency.MaydayFalloff, light);
        }

        var instrumentQuery = EntityQueryEnumerator<InstrumentComponent>();
        while (instrumentQuery.MoveNext(out var uid, out _))
        {
            if (Station.GetOwningStation(uid) != station.Owner)
                continue;

            _userInterface.CloseUis(uid);
        }

        var jukeboxQuery = EntityQueryEnumerator<JukeboxComponent>();
        while (jukeboxQuery.MoveNext(out var uid, out var jukebox))
        {
            if (Station.GetOwningStation(uid) != station.Owner)
                continue;

            _jukebox.Stop((uid, jukebox));
        }
    }

    protected override void Scuttling(Entity<DVStationKeycardAuthenticationDeviceComponent> station)
    {
        base.Scuttling(station);

        _nukeCodePaper.SendNukeCodes(station);
    }
}
