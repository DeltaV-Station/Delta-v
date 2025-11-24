using Content.Server.Discord.DiscordLink;
using NetCord;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server._DV.Discord;
public sealed partial class DiscordUserLink
{
    private readonly ulong[] _patreonRoleIds = new[]
    {
        (ulong) 1440410487002239017,
        (ulong) 1429160196902748211, // Nuclear Operative
    };

    public bool IsPatreon(NetUserId userId)
    {
        var link = GetLink(userId);

        if (link is not { } discordLink)
        {
            return false;
        }

        var guildUser = GetDiscordIdAsUser(discordLink.DiscordUserId);

        if (guildUser == null)
        {
            return false;
        }

        return guildUser.RoleIds.Any(roleId => _patreonRoleIds.Contains(roleId));
    }

    public ActiveDiscordLink? GetLink(NetUserId userId) => _links.FirstOrNull(link => link.UserId == userId);

    public ActiveDiscordLink? GetLink(ulong discordId) => _links.FirstOrNull(link => link.DiscordUserId == discordId);

    public GuildUser? GetDiscordIdAsUser(ulong discordId)
    {
        if (_discordLink.Client == null || !_discordLink.Client.Cache.Guilds.TryGetValue(_discordLink.GuildId, out var guild))
            return null;

        return guild.Users.TryGetValue(discordId, out var user) ? user : null;
    }

    private async Task SendDirectMessage(ulong userId, string message)
    {
        if (_discordLink.Client == null)
        {
            return;
        }

        var dm = await _discordLink.Client.Rest.GetDMChannelAsync(userId);
        await dm.SendMessageAsync(message);
    }

    private bool IsDirectMessage(CommandReceivedEventArgs command)
    {
        return command.Message.Guild == null;
    }

    private async void UpdatePlayerLink(ulong associateDiscordId, ulong? newDiscordId)
    {
        await _dbManager.UpdateDiscordLink(associateDiscordId, newDiscordId);
    }

    private async void UpdatePlayerLink(NetUserId userId, ulong? newDiscordId)
    {
        await _dbManager.UpdateDiscordLink(userId, newDiscordId);
    }

    private string GetRandomCode(int length = CodeLength)
    {
        var code = String.Empty;
        using (var random = RandomNumberGenerator.Create())
        {
            var bytes = new byte[length];
            random.GetBytes(bytes);
            StringBuilder stringBuilder = new();

            foreach (var byteValue in bytes)
            {
                stringBuilder.Append(CodeLetters[byteValue % CodeLetters.Length]);
            }

            code = stringBuilder.ToString();
        }

        return code;
    }


    private string StartVerify(ulong userId)
    {
        var code = GetRandomCode();
        var pendingLink = new PendingLink(userId, code);

        _pendingLinks.RemoveWhere(link => link.DiscordUserId == userId);
        _pendingLinks.Add(pendingLink);

        return code;
    }
}

public record struct PendingLink(ulong InDiscordUserId, string InCode)
{
    public ulong DiscordUserId { get; init; } = InDiscordUserId;
    public string Code { get; init; } = InCode;
}

public record struct ActiveDiscordLink(NetUserId UserId, ulong DiscordUserId)
{
    public NetUserId InGameUserId { get; init; } = UserId;
    public ulong DiscordUserId { get; init; } = DiscordUserId;
}
