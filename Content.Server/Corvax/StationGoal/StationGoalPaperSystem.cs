using System.Data;
using System.Text.RegularExpressions;
using Content.Server.Corvax.GameTicking;
using Content.Shared.Paper;
using Content.Server.Fax;
using Content.Server.GameTicking.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Fax.Components;
using Content.Shared.Corvax.CCCVars;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Configuration;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Corvax.StationGoal;



//░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░▒▓▒▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░▒██████▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░▓█████████▒░░░░░░░░░░░░░░░░░░░░░░░░▒█████▓░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░▓███████████▒░░░░░░░░░░░░░░░░░░░▒▓███████▓░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░▓█████████████▓▒░░░░░░░░░░░░░▒▓██████████░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░▒████████████████▓█▓░░░░░▓███▓███▒░█████▓░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░▓█████▒▒███░▒██████░░░▒█████░░▒░▒██████░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░███████▓▒▒▒████████▓██████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░███████████████████████████████████▓░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░▒███████████████▓░░░▒█████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓██████████████░░░░░░█▓█████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓█████████▒░░░░░░░░░░█▓█████▓█▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░█████▓▒░▒▓░░░░░░░░░░░░░░░▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░██▒░░░░░░░░░░░░░░▒▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒█▓░░░░░▒▒░░░░░░░░//
//░░░░░░░░░░░░░░░░████▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓██▓░░░░░▓▓░░░░░░░░//
//░░░░░░░░▒██▓▒▒░░░▓███▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒███▓░░░░░▒▓▒░░░░░░░░//
//░░░░░░░░▒██████▒░░░▓████▒░░░░░░░░░░░░░░░▒▓██░░░███▓░░░░░░░░░░░░░░░▒▓███▓░░░░░░▓▓▒░░░░░░░░░//
//░░░░░░░░░▓███▓▓▒░░░░▒█████▓▒░░░░░░░░░▒██████▒░░█████▓▒░░░░░░░░░▒▓█████░░░░░░▒██░░░░░░░░░░░//
//░░░░░░░░░░▒███▓▓░░░░░░▒▓███████████████████▒░░░▒███████████████████▓▒░░░░░░▒█▓░░░░░░░░░░░░//
//░░░░░░░░░░░▓███▒░░░░░░░░░░▓▓███████████▓▓▒░░░░░░░░▒███████████▓▒▒░░░░░░░▒▒▓██▒░░░░░░░░░░░░//
//░░░░░░░░░░░▒▓██▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▒▒▒░░░░░░░░░▒▓██████▒░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░▓▓▓▓▓▓█▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▓████████▓▒░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░▒████▓░░░▓██▓▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▓█████████▒░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░▓████████████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒▓▓▓▓██████████████▓░░░░░░░░░░░░░░░░░░░░░░//
//░░░░░░░░░░░░░░░░░░░▒█████████████████████████████████████████████▓▒░░░░░░░░░░░░░░░░░░░░░░░//
//▓▒░░░░░░░░░░░░░░░░░░░▒▒▒▓█████████████████████████████████████▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░//
//████▓▒░░░░░░░░░░░░░░░░░░░░░░░▒▓▓███████████████████████████▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░▓██//
//███████▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▒▒▒▒▒▒▓▓▓▒▒▒▒▒▒▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▓█████//
//███████████▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▓█████████//
//███████████████▓▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▓█████████████//
//███████████████████▓▓▒░░░░░░░░░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒░░░░░░░░░▒▓▓█████████████████//
//████████████████████████▒░░░░░▒██████████████████████████████▒░░░░░▒▓█████████████████████//
//██████████████████████████▓▒░░░███████████▓▓▓▓▓▓▓▓███████████░░░▒█████████████████████████//
//█████████████████████████████▓░░▓██████░░░░░░▒░░░░░░░███████░░▓███████████████████████████//
//███████████████████████████████▓░░░▒▓██░░░░░░░░░░░░░▒██▓▒░░░▓█████████████████████████████//
//█████████████████████████████████▓██████▒░░░░░░░░░░▒██████████████████████████████████████//
//█████████████████████████████████████████▓░░░░░░░░▓███████████████████████████████████████//
//████████████████████████████████████████▒▓▒░░░░░░▓▓▒██████████████████████████████████████//
//█████████████████████████████████████████▒░░░░░░░░░███████████████████████████████████████//
//██████████████████████████████████████████▓░░░░░░▓████████████████████████████████████████//

// Любят же корвахи использовать свои ивенты
// А потом из-за них ошибки исправлять приходится

/// <summary>
///     System for station goals
/// </summary>
public sealed class StationGoalPaperSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FaxSystem _fax = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private static readonly Regex StationIdRegex = new(@".*-(\d+)$");

    private const string RandomPrototype = "StationGoals";

    /// <summary>
    ///     Send a random station goal to all faxes which are authorized to receive it
    /// </summary>
    /// <returns>If the fax was successful</returns>
    /// <exception cref="ConstraintException">Raised when station goal types in the prototype is invalid</exception>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        var playerCount = _playerManager.PlayerCount;

        var query = EntityQueryEnumerator<StationGoalComponent>();
        while (query.MoveNext(out var uid, out var station))
        {
            var tempGoals = new List<ProtoId<StationGoalPrototype>>(station.Goals);
            StationGoalPrototype? selGoal = null;
            while (tempGoals.Count > 0)
            {
                var goalId = _random.Pick(tempGoals);
                var goalProto = _proto.Index(goalId);

                if (playerCount > goalProto.MaxPlayers ||
                    playerCount < goalProto.MinPlayers)
                {
                    tempGoals.Remove(goalId);
                    continue;
                }

                selGoal = goalProto;
                break;
            }

            if (selGoal is null)
                return;

            if (SendStationGoal(uid, selGoal))
            {
                Log.Info($"Goal {selGoal.ID} has been sent to station {MetaData(uid).EntityName}");
            }
        }
    }

    public bool SendStationGoal(EntityUid? ent, ProtoId<StationGoalPrototype> goal)
    {
        return SendStationGoal(ent, _proto.Index(goal));
    }

    /// <summary>
    ///     Send a station goal to all faxes which are authorized to receive it.
    ///     Send a station goal on selected station to all faxes which are authorized to receive it.
    /// </summary>
    /// <returns>True if at least one fax received paper</returns>
    public bool SendStationGoal(EntityUid? ent, StationGoalPrototype goal)
    {
        if (ent is null)
            return false;

        if (!TryComp<StationDataComponent>(ent, out var stationData))
            return false;
        //Logger.Debug(MetaData(ent.Value).EntityName);
        //string agfaeg = Loc.GetString(goal.Text, ("station", (string)MetaData(ent.Value).EntityName));
        //Log.Debug(MetaData(ent.Value).EntityName);
        var printout = new FaxPrintout(

            Loc.GetString("station-goal-fax-paper-header", ("station", (string)MetaData(ent.Value).EntityName), ("date", DateTime.Now.AddYears(1000).ToString("yyyy MMMM dd")), ("content", Loc.GetString(goal.Text))),
            Loc.GetString("station-goal-fax-paper-name"),
            null,
            null,
            "paper_stamp-centcom",
            new List<StampDisplayInfo>
            {
                new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") },
            });

        var wasSent = false;
        var query = EntityQueryEnumerator<FaxMachineComponent>();
        while (query.MoveNext(out var faxUid, out var fax))
        {
            if (!fax.ReceiveStationGoal) continue;
            if (!fax.ReceiveStationGoal)
                continue;

            var largestGrid = _station.GetLargestGrid(stationData);
            var grid = Transform(faxUid).GridUid;
            if (grid is not null && largestGrid == grid.Value)
            {
                _fax.Receive(faxUid, printout, null, fax);
                foreach (var spawnEnt in goal.Spawns)
                {
                    SpawnAtPosition(spawnEnt, Transform(faxUid).Coordinates);
                }
                wasSent = true;
            }
        }

        return wasSent;
    }
}
