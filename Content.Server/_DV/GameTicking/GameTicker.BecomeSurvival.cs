using System.Linq;
using Content.Server.Administration;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Content.Shared.Fax;
using Content.Server.StationEvents.Components;

namespace Content.Server.GameTicking;

/// <summary>
/// Extends upstream's <see cref="GameTicker" />.
/// </summary>
public sealed partial class GameTicker
{
    private static readonly EntProtoId RampingSchedulerProto = "RampingStationEventScheduler";

    /// <summary>
    /// DeltaV - Removes the basic scheduler and adds a ramping scheduler to the round. Does nothing
    /// if there is already a ramping scheduler.
    /// </summary>
    /// <returns>True if the game rules either contain or added a RampingStationEventScheduler.</returns>
    [PublicAPI]
    public bool ConvertRoundToSurvival()
    {
        _chatManager.SendAdminAlert("Major antag defeated. Converting to survival.");

        // Ramping scheduler is already added. Do nothing.
        if (IsGameRuleActive(RampingSchedulerProto))
        {
            _chatManager.SendAdminAlert("RampingStationEventScheduler detected. No rules added.");
            return true;
        }

        var basicRules = EntityQueryEnumerator<BasicStationEventSchedulerComponent>();
        while (basicRules.MoveNext(out var uid, out var _))
            EndGameRule(uid);

        var rampingScheduler = AddGameRule(RampingSchedulerProto);
        StartGameRule(rampingScheduler);

        return IsGameRuleActive(RampingSchedulerProto);
    }
}
