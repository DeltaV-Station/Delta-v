using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace Content.DiscordBot.Modules;

public sealed class AccountLinkingModule : ModuleBase<SocketCommandContext>
{
    [SlashCommand("create", "Creates the message with the linking popup. Only has to be run once.")]
    [Discord.Interactions.RequireOwner]
    public Task CreateAsync()
    {
        var component = new ComponentBuilder()
            .WithButton("Link your SS14 account here!", "link-ss14-account")
            .Build();

        return ReplyAsync(string.Empty, components: component);
    }
}
