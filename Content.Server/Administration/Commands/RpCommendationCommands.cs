using System.Linq;
using System.Text;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

/// <summary>
/// Shows the top-10 players by RP commendation count for the last 7 days.
/// Usage: rp_top [days]
/// </summary>
[AdminCommand(AdminFlags.Moderator)]
public sealed class RpTopCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public string Command => "rp_top";
    public string Description => "Shows the top-10 players by RP commendation count.";
    public string Help => "Usage: rp_top [days=7]";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var days = 7;
        if (args.Length >= 1 && !int.TryParse(args[0], out days))
        {
            shell.WriteError("Invalid number of days.");
            return;
        }

        var window = TimeSpan.FromDays(days);
        var top = await _db.GetTopCommendations(10, window);

        if (top.Count == 0)
        {
            shell.WriteLine($"No commendations found in the last {days} days.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"--- RP Commendation Top-10 (last {days} days) ---");
        var rank = 1;
        foreach (var (userId, userName, count) in top)
        {
            sb.AppendLine($"  #{rank}: {userName} — {count} commendation(s)");
            rank++;
        }

        shell.WriteLine(sb.ToString());
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint("Number of days (default: 7)");

        return CompletionResult.Empty;
    }
}

/// <summary>
/// Admin command to manually award RP commendation points to a player.
/// Usage: rp_award <username> <count> <reason>
/// </summary>
[AdminCommand(AdminFlags.Moderator)]
public sealed class RpAwardCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "rp_award";
    public string Description => "Manually award RP commendation points to a player.";
    public string Help => "Usage: rp_award <username> <count> \"<reason>\"";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 3)
        {
            shell.WriteError("Usage: rp_award <username> <count> <reason>");
            return;
        }

        var userName = args[0];

        if (!int.TryParse(args[1], out var count) || count <= 0)
        {
            shell.WriteError("Count must be a positive integer.");
            return;
        }

        var reason = string.Join(" ", args.Skip(2));

        if (!_playerManager.TryGetSessionByUsername(userName, out var player))
        {
            shell.WriteError($"Player '{userName}' not found (must be online).");
            return;
        }

        var roundId = _entityManager.System<GameTicker>().RoundId;
        await _db.AddAdminCommendation(roundId, player.UserId.UserId, count, reason);

        shell.WriteLine($"Awarded {count} commendation(s) to {userName}. Reason: {reason}");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player username");

        if (args.Length == 2)
            return CompletionResult.FromHint("Number of points to award");

        if (args.Length == 3)
            return CompletionResult.FromHint("Reason for the award");

        return CompletionResult.Empty;
    }
}

/// <summary>
/// Admin command to wipe the weekly RP commendation data.
/// Usage: rp_wipe_weekly [confirm]
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed class RpWipeWeeklyCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public string Command => "rp_wipe_weekly";
    public string Description => "Wipes all RP commendation data (weekly reset).";
    public string Help => "Usage: rp_wipe_weekly confirm";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1 || args[0] != "confirm")
        {
            shell.WriteError("This will DELETE all commendation records. Run 'rp_wipe_weekly confirm' to proceed.");
            return;
        }

        var deleted = await _db.WipeAllCommendations();
        shell.WriteLine($"Wiped {deleted} commendation record(s).");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint("Type 'confirm' to wipe all commendation data");

        return CompletionResult.Empty;
    }
}

/// <summary>
/// Admin command to view the commendation log (who voted for whom).
/// Usage: rp_log [days]
/// </summary>
[AdminCommand(AdminFlags.Moderator)]
public sealed class RpLogCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public string Command => "rp_log";
    public string Description => "Shows the RP commendation vote log (who commended whom).";
    public string Help => "Usage: rp_log [days=7]";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var days = 7;
        if (args.Length >= 1 && !int.TryParse(args[0], out days))
        {
            shell.WriteError("Invalid number of days.");
            return;
        }

        var window = TimeSpan.FromDays(days);
        var log = await _db.GetCommendationLog(window);

        if (log.Count == 0)
        {
            shell.WriteLine($"No commendation votes found in the last {days} days.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"--- RP Commendation Log (last {days} days, {log.Count} entries) ---");
        foreach (var (senderName, receiverName, roundId, time) in log)
        {
            sb.AppendLine($"  Round {roundId} | {time:yyyy-MM-dd HH:mm} | {senderName} -> {receiverName}");
        }

        shell.WriteLine(sb.ToString());
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint("Number of days (default: 7)");

        return CompletionResult.Empty;
    }
}
