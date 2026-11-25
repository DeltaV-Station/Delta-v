using System.Linq;
using Content.Server._DV.Cabinet;
using Content.Server._DV.Station.Components;
using Content.Server._DV.Station.Events;
using Content.Server.Chat.Systems;
using Content.Server.NukeOps;
using Content.Server.Station.Components;
using Content.Shared._DV.CCVars;
using Content.Shared.Access.Components;
using Content.Shared.Access;
using Content.Shared.NukeOps;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Station.Systems;

public sealed class AutomaticSpareIdSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private bool _autoUnlock;
    private TimeSpan _alertDelay;
    private TimeSpan _unlockDelay;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutomaticSpareIdComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AutomaticSpareIdComponent, PlayerJobAddedEvent>(OnPlayerJobAdded);
        SubscribeLocalEvent<AutomaticSpareIdComponent, PlayerJobsRemovedEvent>(OnPlayerJobsRemoved);
        SubscribeLocalEvent<WarDeclaredEvent>(OnWarDeclared);

        Subs.CVar(_cfg, DCCVars.SpareIdAutoUnlock, a => _autoUnlock = a, true);
        Subs.CVar(_cfg, DCCVars.SpareIdAlertDelay, a => _alertDelay = a, true);
        Subs.CVar(_cfg, DCCVars.SpareIdUnlockDelay, a => _unlockDelay = a, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AutomaticSpareIdComponent>();
        while (query.MoveNext(out var station, out var spareId))
        {
            if (spareId.Timeout is { } timeout && _timing.CurTime > timeout)
            {
                Timeout((station, spareId));
            }
        }
    }

    private void Timeout(Entity<AutomaticSpareIdComponent> ent)
    {
        if (ent.Comp.State is AutomaticSpareIdState.RoundStart)
        {
            if (HasCaptain(ent))
                RoundStartCaptain(ent);
            else
                RoundStartNoCaptain(ent);
        }
        else if (ent.Comp.State is AutomaticSpareIdState.AwaitingUnlock)
        {
            MoveToUnlocked(ent);
        }
        else if (ent.Comp.State is AutomaticSpareIdState.WarOps)
        {
            // Default to these, then check if there is a captain
            var message = ent.Comp.WarOpsUnlockedMessageACO;
            var accessGranted = ent.Comp.GrantAccessToCommand;
            if (HasCaptain(ent))
            {
                message = ent.Comp.WarOpsUnlockedMessageCaptain;
                accessGranted = ent.Comp.GrantAccessToCaptain;
            }
            ent.Comp.State = AutomaticSpareIdState.AwaitingUnlock;
            MoveToUnlocked(ent, accessGranted, message);
        }
        else
        {
            DebugTools.Assert($"Spare ID state timed out with unexpected state {ent.Comp.State}");
        }
    }

    private void RoundStartNoCaptain(Entity<AutomaticSpareIdComponent> ent)
    {
        if (_autoUnlock)
            MoveToAwaitingUnlock(ent);
        else
            MoveToAlerted(ent);
    }

    private static void RoundStartCaptain(Entity<AutomaticSpareIdComponent> ent)
    {
        ent.Comp.State = AutomaticSpareIdState.CaptainPresent;
        ent.Comp.Timeout = null;
    }

    private void OnMapInit(Entity<AutomaticSpareIdComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Timeout = _timing.CurTime + _alertDelay;
    }

    private void OnPlayerJobAdded(Entity<AutomaticSpareIdComponent> ent, ref PlayerJobAddedEvent args)
    {
        if (args.JobPrototypeId == ent.Comp.CaptainJob)
            MoveToCaptainPresent(ent);
    }

    private void OnPlayerJobsRemoved(Entity<AutomaticSpareIdComponent> ent, ref PlayerJobsRemovedEvent args)
    {
        if (!args.PlayerJobs.Contains(ent.Comp.CaptainJob) || HasCaptain(ent))
            return;

        MoveToAlerted(ent);
    }

    private void OnWarDeclared(ref WarDeclaredEvent args)
    {
        if (args.Status == WarConditionStatus.YesWar)
        {
            foreach (var spareId in EntityQuery<AutomaticSpareIdComponent>())
            {
                spareId.Timeout = _timing.CurTime + spareId.WarOpsUnlockDelay;
                spareId.State = AutomaticSpareIdState.WarOps;
            }
        }
    }

    private bool HasCaptain(Entity<AutomaticSpareIdComponent> ent)
    {
        if (!TryComp<StationJobsComponent>(ent, out var stationJobs))
            return false;

        return stationJobs.PlayerJobs.Any(playerJobs => playerJobs.Value.Contains(ent.Comp.CaptainJob));
    }

    private void MoveToAwaitingUnlock(Entity<AutomaticSpareIdComponent> ent)
    {
        DebugTools.Assert(ent.Comp.State is AutomaticSpareIdState.RoundStart, $"Spare ID state has unexpected state {ent.Comp.State} on awaiting to unlock");

        ent.Comp.State = AutomaticSpareIdState.AwaitingUnlock;
        ent.Comp.Timeout = _timing.CurTime + _unlockDelay;

        _chat.DispatchStationAnnouncement(ent, Loc.GetString(ent.Comp.AwaitingUnlockMessage, ("minutes", _unlockDelay.TotalMinutes)), colorOverride: Color.Gold);
    }

    /// <summary>
    ///     Unlocks all spare ID cabinets, giving access to a certain access prototype and displays a message to the station.
    /// </summary>
    /// <param name="ent">The station entity that has the <see cref="AutomaticSpareIdComponent"/>.</param>
    /// <param name="newSpareIdAccess">The access to give to the spare ID cabinet. If not specified or null, will default to ent.Comp.GrantAccessToCommand</param>
    /// <param name="unlockMessageLocId">The message to display to the station upon unlocking the spare ID. If not specified or null, will default to ent.Comp.UnlockedMessage</param>
    private void MoveToUnlocked(Entity<AutomaticSpareIdComponent> ent, ProtoId<AccessLevelPrototype>? newSpareIdAccess = null, LocId? unlockMessageLocId = null)
    {
        DebugTools.Assert(ent.Comp.State is AutomaticSpareIdState.AwaitingUnlock, $"Spare ID state has unexpected state {ent.Comp.State} on unlocking");

        ent.Comp.State = AutomaticSpareIdState.Unlocked;
        ent.Comp.Timeout = null;

        var query = EntityQueryEnumerator<SpareIDSafeComponent, AccessReaderComponent>();
        while (query.MoveNext(out var uid, out _, out var accessReader))
        {
            var accesses = accessReader.AccessLists;
            if (accesses.Count <= 0)
                continue;

            if (!newSpareIdAccess.HasValue)
                newSpareIdAccess = ent.Comp.GrantAccessToCommand; // Default to command if no access is specified

            accesses.Add([newSpareIdAccess.Value]);
            Dirty(uid, accessReader);
            RaiseLocalEvent(uid, new AccessReaderConfigurationChangedEvent());
        }

        if (!unlockMessageLocId.HasValue)
            unlockMessageLocId = ent.Comp.UnlockedMessage; // Default message if nothing is specified

        _chat.DispatchStationAnnouncement(ent, Loc.GetString(unlockMessageLocId), colorOverride: Color.Red);
    }

    private void MoveToCaptainPresent(Entity<AutomaticSpareIdComponent> ent)
    {
        if (!(ent.Comp.State is AutomaticSpareIdState.Alerted or AutomaticSpareIdState.AwaitingUnlock or AutomaticSpareIdState.Unlocked))
        {
            return;
        }

        ent.Comp.State = AutomaticSpareIdState.CaptainPresent;
        ent.Comp.Timeout = null;

        _chat.DispatchStationAnnouncement(ent, Loc.GetString(ent.Comp.CaptainPresentAfterAlertsMessage), colorOverride: Color.Gold);
    }

    private void MoveToAlerted(Entity<AutomaticSpareIdComponent> ent)
    {
        DebugTools.Assert(ent.Comp.State is AutomaticSpareIdState.RoundStart or AutomaticSpareIdState.CaptainPresent, $"Spare ID state has unexpected state {ent.Comp.State} on moving to alerted");

        ent.Comp.State = AutomaticSpareIdState.Alerted;
        ent.Comp.Timeout = null;

        _chat.DispatchStationAnnouncement(ent, Loc.GetString(ent.Comp.AlertedMessage), colorOverride: Color.Gold);
    }
}
