using Content.Shared.Administration;
using NetCord.Gateway;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Server._DV.Discord;
public sealed partial class DiscordUserLink
{
    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs eventArgs)
    {
        if (eventArgs.NewStatus != SessionStatus.Connected)
        {
            return;
        }

        await SetupPlayerAsync(eventArgs.Session.UserId);
    }

    private async Task SetupPlayerAsync(NetUserId userId)
    {
        var link = await _dbManager.GetDiscordLink(userId);

        if (link is not { } discordId || _links.Any(targetlink => targetlink.DiscordUserId == discordId))
        {
            return;
        }

        if (_discordLink.Client != null &&
            _discordLink.Client.Cache.Guilds.TryGetValue(_discordLink.GuildId, out var guild))
        {
            var guildUser = await _discordLink.Client.Rest.GetGuildUserAsync(_discordLink.GuildId, discordId);
            _discordLink.Client.Cache.CacheGuildUser(guildUser);
        }

        _links.Add(new (userId, discordId));
    }
}

[AnyCommand]
public sealed class VerifyCommand : IConsoleCommand
{
    public string Command => "verify";
    public string Description => "Verify your discord account with your SS14 Account";
    public string Help => "Usage: Verify <code>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var discordUserLink = entityManager.System<DiscordUserLink>();

        if (args.Length < 1 || shell.Player == null)
        {
            return;
        }

        var success = discordUserLink.TryGameVerify(shell.Player.UserId, args[0]);
        var successText = success ? string.Empty : " not";
        shell.WriteLine($"Your discord account has{successText} been verified");
    }
}
