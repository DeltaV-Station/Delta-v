using System.Collections.Immutable;
using System.Reflection;
using Content.DiscordBot.Modules;
using Content.Server.Database;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Content.DiscordBot;

public sealed class CommandHandler(DiscordSocketClient client, InteractionService interaction, ServerDbContext db, Config config)
{
    private ImmutableDictionary<ulong, DeltaVPatronTier>? _patronTiers;
    private ImmutableArray<DeltaVPatronTier> _tierPriority;
    private Task? _refreshPatronsTask;

    public int Running = 1;

    public async Task InstallCommandsAsync()
    {
        var patronTiers = await db.PatronTiers.ToListAsync();
        _tierPriority = [..patronTiers.OrderBy(t => t.Priority)];
        _patronTiers = patronTiers.ToImmutableDictionary(t => t.DiscordRole, t => t);

        client.Ready += ReadyAsync;
        client.SlashCommandExecuted += HandleCommandAsync;
        client.ButtonExecuted += HandleButtonAsync;
        client.ModalSubmitted += HandleModalAsync;
        client.GuildMemberUpdated += HandleGuildMemberUpdated;

        // If you get a log that says '[ERROR]' here, you probably don't have the corrent intents 
        await client.LoginAsync(TokenType.Bot, config.Token);
        await client.StartAsync();

        interaction.AddModalInfo<LinkAccountModal>();

        _refreshPatronsTask = Task.Run(async () => await RefreshPatrons());
    }

    private async Task ReadyAsync()
    {
        await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        await interaction.RegisterCommandsToGuildAsync(config.Guild);
    }

    private async Task HandleGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> old, SocketGuildUser user)
    {
        if (_patronTiers == null)
            return;

        var rolesChanged = !old.HasValue || old.Value.Roles.Count != user.Roles.Count || !old.Value.Roles.SequenceEqual(user.Roles);
        if (!rolesChanged)
            return;

        var wasPatron = old.HasValue || old.Value.Roles.Any(r => _patronTiers.ContainsKey(r.Id));
        var isPatron = user.Roles.Any(r => _patronTiers.ContainsKey(r.Id));
        if (wasPatron && !isPatron)
        {
            var linked = await db.DiscordLinkedAccounts
                .Include(l => l.Player)
                .ThenInclude(p => p.Patron)
                .ThenInclude(p => p!.Tier)
                .Where(l => l.Player.Patron != null)
                .FirstOrDefaultAsync(p => p.DiscordId == user.Id);

            if (linked?.Player.Patron is { } patron)
            {
                db.Patrons.Remove(patron);
                await db.SaveChangesAsync();
                await Logger.Info($"Removed patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName} with tier {patron.Tier.Name}");
            }

            return;
        }

        if (isPatron)
        {
            foreach (var tier in _tierPriority)
            {
                if (user.Roles.Any(r => r.Id == tier.DiscordRole))
                {
                    var linked = await db.DiscordLinkedAccounts
                        .Include(l => l.Player)
                        .ThenInclude(p => p.Patron)
                        .FirstOrDefaultAsync(p => p.DiscordId == user.Id);

                    if (linked?.Player is not { } player)
                        return;

                    player.Patron ??= db.Patrons.Add(new DeltaVPatron { PlayerId = player.UserId }).Entity;
                    player.Patron.TierId = tier.Id;
                    await db.SaveChangesAsync();
                    await Logger.Info($"Updated patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName} with tier {tier.Name}");
                }
            }
        }
    }

    private async Task HandleCommandAsync(SocketSlashCommand message)
    {

        if (message.GuildId != config.Guild)
            return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketInteractionContext(client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await interaction.ExecuteCommandAsync(context, null);
    }

    private async Task HandleButtonAsync(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "link-ss14-account":
                await component.RespondWithModalAsync<LinkAccountModal>("link-ss14-account");
                break;
        }
    }

    private async Task HandleModalAsync(SocketModal modal)
    {
        switch (modal.Data.CustomId)
        {
            case "link-ss14-account":
                if (modal.GuildId is not { } guildId)
                    break;

                var codeStr = modal.Data.Components.First(c => c.CustomId == "account_code").Value.Trim();
                if (string.IsNullOrWhiteSpace(codeStr))
                    break;

                await modal.DeferAsync(true);
                if (!Guid.TryParse(codeStr, out var code))
                {
                    await modal.FollowupAsync($"{codeStr} isn't a valid code! Get one in-game from the lobby at the top left of the screen.", ephemeral: true);
                }

                var author = modal.User;
                var authorId = author.Id;
                var discord = await db.DiscordAccounts
                    .Include(d => d.LinkedAccount)
                    .ThenInclude(l => l.Player)
                    .ThenInclude(p => p.Patron)
                    .FirstOrDefaultAsync(a => a.Id == authorId);
                var codes = await db.DiscordLinkingCodes
                    .Include(l => l.Player)
                    .ThenInclude(player => player.Patron)
                    .FirstOrDefaultAsync(p => p.Code == code);

                if (codes == null)
                {
                    await modal.FollowupAsync($"No player found with code {codeStr}, join the game server and get another code before trying again, or ask for help in another channel.", ephemeral: true);
                    break;
                }

                if (codes.CreationTime < DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                {
                    await modal.FollowupAsync($"Code {codeStr} were generated too long ago, join the game server and get another code before trying again.", ephemeral: true);
                }

                if (discord?.LinkedAccount is { } linked)
                {
                    if (linked.Player.Patron is { } patron)
                        db.Patrons.Remove(patron);

                    linked.Player.Patron = null;
                    db.DiscordLinkedAccounts.Remove(linked);
                }

                discord ??= db.DiscordAccounts.Add(new DiscordAccount { Id = authorId }).Entity;
                discord.LinkedAccount = db.DiscordLinkedAccounts.Add(new DiscordLinkedAccount { Discord = discord }).Entity;
                discord.LinkedAccount.Player = codes.Player;

                var roles = client.GetGuild(guildId).GetUser(authorId).Roles.Select(r => r.Id).ToArray();
                var tiers = await db.PatronTiers
                    .Where(t => roles.Contains(t.DiscordRole))
                    .ToListAsync();
                if (tiers.Count == 0)
                {
                    discord.LinkedAccount.Player.Patron = null;
                }
                else
                {
                    tiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                    var tier = tiers[0];
                    discord.LinkedAccount.Player.Patron = db.Patrons.Add(new DeltaVPatron { Tier = tier }).Entity;
                    discord.LinkedAccount.Player.Patron.Tier = tier;
                }

                db.DiscordLinkedAccountLogs.Add(new DiscordLinkedAccountLogs
                {
                    Discord = discord,
                    Player = discord.LinkedAccount.Player,
                });

                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync();

                var msg = $"Linked SS14 account with name {codes.Player.LastSeenUserName}";
                if (codes.Player.Patron != null)
                    msg += $" and tier {codes.Player.Patron.Tier.Name}";

                await modal.FollowupAsync(msg, ephemeral: true);
                break;
        }
    }

    private async Task RefreshPatrons()
    {
        while (Interlocked.CompareExchange(ref Running, 1, 1) == 1)
        {
            try
            {
                var patrons = await db.DiscordLinkedAccounts
                    .Include(l => l.Player)
                    .ThenInclude(p => p.Patron)
                    .ThenInclude(p => p!.Tier)
                    .ToListAsync();

                foreach (var linked in patrons)
                {
                    try
                    {
                        var user = await client.Rest.GetGuildUserAsync(config.Guild, linked.DiscordId);
                        if (user == null)
                        {
                            if (linked.Player.Patron != null)
                            {
                                linked.Player.Patron = null;
                                await Logger.Info($"Removed patron {linked.DiscordId}:{linked.Player.LastSeenUserName}");
                            }

                            continue;
                        }

                        var isPatron = false;
                        foreach (var tier in _tierPriority)
                        {
                            if (user.RoleIds.Contains(tier.DiscordRole))
                            {
                                isPatron = true;
                                if (linked.Player.Patron?.Tier.DiscordRole == tier.DiscordRole)
                                    break;

                                linked.Player.Patron ??= db.Patrons.Add(new DeltaVPatron { PlayerId = linked.PlayerId })
                                    .Entity;
                                linked.Player.Patron.TierId = tier.Id;
                                await Logger.Info($"Updated patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName} with tier {tier.Name}");
                                break;
                            }
                        }

                        if (!isPatron && linked.Player.Patron != null)
                        {
                            linked.Player.Patron = null;
                            await Logger.Info($"Removed patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName}");
                        }
                    }
                    catch (Exception e)
                    {
                        await Logger.Error($"Error updating patron with discord id {linked.DiscordId} and player id {linked.PlayerId}", e);
                    }
                }

                await db.SaveChangesAsync();
                await Task.Delay(60000); // 60 seconds
            }
            catch (Exception e)
            {
                await Logger.Error("Error refreshing patrons", e);
            }
        }
    }
}
