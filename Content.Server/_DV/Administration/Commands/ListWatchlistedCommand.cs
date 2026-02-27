using System.Globalization;
using Content.Server.Administration;
using Content.Server.Administration.Notes;
using Content.Server.Commands;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Silicons.Laws.Components;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class ListWatchlistedCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IAdminNotesManager _notes = default!;

    public override string Command => "lswatchlisted";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var found = false;
        foreach (var sessionData in _player.GetAllPlayerData())
        {
            var watchlists = await _notes.GetActiveWatchlists(sessionData.UserId.UserId);
            if (watchlists.Count == 0)
                return;

            found = true;

            shell.WriteLine("");
            shell.WriteLine(sessionData.UserName);
            foreach (var record in watchlists)
            {
                shell.WriteLine("");
                record.CreatedAt.Deconstruct(out var date, out _, out _);
                shell.WriteLine($"Created: {date.ToString("O", CultureInfo.InvariantCulture)}");
                if (record.ExpirationTime is { } expirationTime)
                {
                    expirationTime.Deconstruct(out var expirationDate, out _, out _);
                    shell.WriteLine($"Expires: {expirationDate.ToString("O", CultureInfo.InvariantCulture)}");
                }
                else
                {
                    shell.WriteLine("Expires: PERMANENT");
                }
                foreach (var line in record.Message.Split('\n', StringSplitOptions.TrimEntries))
                {
                   shell.WriteLine($">  {line}");
                }
            }
        }

        if (!found)
        {
            shell.WriteLine("No watchlisted players online.");
        }
    }
}
