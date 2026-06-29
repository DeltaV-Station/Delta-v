using Discord;
using Discord.Interactions;

namespace Content.DiscordBot.Modules;

public sealed class AccountLinkingModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("discordlink", "Creates the message with the linking button.")]
    [RequireTeam]
    public Task CreateAsync()
    {
        var component = new ComponentBuilder()
            .WithButton("Link your SS14 account here!", "link-ss14-account")
            .Build();

        return RespondAsync(String.Empty, components: component);
    }
}