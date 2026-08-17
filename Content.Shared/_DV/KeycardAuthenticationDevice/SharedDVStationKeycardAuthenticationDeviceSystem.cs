using Content.Shared.Chat;
using Content.Shared.Electrocution;
using Content.Shared.Station;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Shared._DV.KeycardAuthenticationDevice;

public abstract class SharedDVStationKeycardAuthenticationDeviceSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly SharedStationSystem Station = default!;
    [Dependency] private readonly SharedElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DVStationKeycardAuthenticationDeviceComponent>();
        while (query.MoveNext(out var uid, out var keycard))
        {
            if (keycard.SwipesStartedAt is not { } startedAt || _timing.CurTime <= startedAt + keycard.SwipeWindow)
                continue;

            StopSwipes((uid, keycard), true);
        }
    }

    private void StopSwipes(Entity<DVStationKeycardAuthenticationDeviceComponent> station, bool failed)
    {
        station.Comp.SwipesStartedAt = null;
        station.Comp.Swipes = 0;
        station.Comp.SwipingFor = null;
        if (failed)
        {
            station.Comp.AccessibleAfter = _timing.CurTime + station.Comp.FailureDelay;
            _chat.DispatchStationAnnouncement(station,
                Loc.GetString(station.Comp.FailureAnnouncement),
                Loc.GetString(station.Comp.FailureAnnouncementSender),
                announcementSound: new SoundPathSpecifier("/Audio/_DV/Announcements/attention.ogg"),
                colorOverride: station.Comp.FailureAnnouncementColor);
        }
        Dirty(station);

        var query = EntityQueryEnumerator<DVStationKeycardAuthenticationDeviceAlreadySwipedComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (failed)
            {
                _electrocution.TryDoElectrocution(uid,
                    null,
                    10,
                    station.Comp.FailureElectrocutionDuration,
                    true,
                    ignoreInsulation: true);
            }
            RemCompDeferred<DVStationKeycardAuthenticationDeviceAlreadySwipedComponent>(uid);
        }
    }

    private void Swipe(Entity<DVStationKeycardAuthenticationDeviceComponent> station, EntityUid user, DVStationKeycardAction action)
    {
        if (!station.Comp.ActionThresholds.TryGetValue(action, out var threshold))
            return;

        station.Comp.SwipesStartedAt ??= _timing.CurTime;
        station.Comp.SwipingFor = action;
        station.Comp.Swipes++;
        AddComp<DVStationKeycardAuthenticationDeviceAlreadySwipedComponent>(user);
        Dirty(station);

        if (station.Comp.Swipes >= threshold)
        {
            StopSwipes(station, false);
            DoAction(station, action);
        }
    }

    private void DoAction(Entity<DVStationKeycardAuthenticationDeviceComponent> station, DVStationKeycardAction action)
    {
        switch (action)
        {
            case DVStationKeycardAction.Mayday:
                Mayday(station);
                break;

            case DVStationKeycardAction.Scuttling:
                Scuttling(station);
                break;
        }
    }

    protected virtual void Mayday(Entity<DVStationKeycardAuthenticationDeviceComponent> station)
    {
    }

    protected virtual void Scuttling(Entity<DVStationKeycardAuthenticationDeviceComponent> station)
    {
    }

    public void TrySwipe(EntityUid user, EntityUid console, DVStationKeycardAction action)
    {
        if (HasComp<DVStationKeycardAuthenticationDeviceAlreadySwipedComponent>(console))
            return;

        if (Station.GetOwningStation(user) is not { } station || !TryComp<DVStationKeycardAuthenticationDeviceComponent>(station, out var keycard))
            return;

        if (keycard.SwipingFor is { } swipingFor && swipingFor != action)
            return;

        if (_timing.CurTime <= keycard.AccessibleAfter)
            return;

        Swipe((station, keycard), user, action);
    }
}
