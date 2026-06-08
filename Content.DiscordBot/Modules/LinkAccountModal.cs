using Discord.Interactions;

namespace Content.DiscordBot.Modules;

public class LinkAccountModal : IModal
{
    public string Title => "Link SS14 Account";

    [InputLabel("SS14 Linking Code (Top Left in the Lobby)")]
    [RequiredInput]
    [ModalTextInput("account_code")]
    public string Code { get; set; } = string.Empty;
}
