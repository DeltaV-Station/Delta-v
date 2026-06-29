using Robust.Shared.Configuration;

namespace Content.Shared._DV.CCVars;

public sealed partial class DCCVars
{
    /// <summary>
    /// Redirects you to the channel in discord to link your account.
    /// </summary>
    public static readonly CVarDef<string> DiscordAccountLinkingMessageLink =
        CVarDef.Create("discord.discord_account_linking_message_link", "", CVar.REPLICATED | CVar.SERVER);

}
