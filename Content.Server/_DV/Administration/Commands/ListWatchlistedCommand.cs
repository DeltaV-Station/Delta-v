using System.Globalization;
using Content.Server.Administration;
using Content.Server.Administration.Notes;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class ListWatchlistedCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly AdminNotesSystem _notes = default!;

    public override string Command => "lswatchlisted";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_notes.ConnectedPlayerWatchlists.Count == 0)
        {
            shell.WriteLine("No watchlisted players online.");
            return;
        }

        foreach (var (playerId, records) in _notes.ConnectedPlayerWatchlists)
        {
            if (!_player.TryGetSessionById(playerId, out var sessionData))
                return;

            shell.WriteMarkup($"\n[bold]{sessionData.Name}[/bold]\n");
            foreach (var record in records)
            {
                shell.WriteLine("");
                record.CreatedAt.Deconstruct(out var date, out _);
                shell.WriteLine($"Created: {date.ToString("O", CultureInfo.InvariantCulture)}");
                if (record.ExpiryTime is { } expirationTime)
                {
                    expirationTime.Deconstruct(out var expirationDate, out _);
                    shell.WriteLine($"Expires: {expirationDate.ToString("O", CultureInfo.InvariantCulture)}");
                }
                else
                {
                    shell.WriteLine("Expires: PERMANENT");
                }

                foreach (var line in record.Message.Split('\n', StringSplitOptions.TrimEntries))
                {
                    shell.WriteLine($"> {line}");
                }
            }
        }
    }
}
