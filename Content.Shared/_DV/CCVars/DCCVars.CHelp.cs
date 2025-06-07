using Robust.Shared.Audio;
using Robust.Shared.Configuration;

namespace Content.Shared._DV.CCVars;

public sealed partial class DCCVars
{
    /*
    * Curator Help
    */
    public static readonly CVarDef<SoundPathSpecifier> CHelpSound =
        CVarDef.Create("audio.chelp_sound", new SoundPathSpecifier("/Audio/_RMC14/Effects/Admin/mhelp.ogg"), CVar.ARCHIVE | CVar.CLIENTONLY);

    /// <summary>
    ///     URL of the Discord webhook which will relay all chelp messages.
    /// </summary>
    public static readonly CVarDef<string> DiscordCHelpWebhook =
        CVarDef.Create("discord.chelp_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     The server icon to use in the Discord chelp embed footer.
    ///     Valid values are specified at https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure.
    /// </summary>
    public static readonly CVarDef<string> DiscordCHelpFooterIcon =
        CVarDef.Create("discord.chelp_footer_icon", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     The avatar to use for the chelp webhook. Should be an URL.
    /// </summary>
    public static readonly CVarDef<string> DiscordCHelpAvatar =
        CVarDef.Create("discord.chelp_avatar", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Use the curator's Admin OOC color in cwoinks.
    ///     If either the ooc color or this is not set, uses the admin.curator_cwoink_color value.
    /// </summary>
    public static readonly CVarDef<bool> UseAdminOOCColorInCwoinks =
        CVarDef.Create("admin.cwoink_use_admin_ooc_color", true, CVar.SERVERONLY);

    /// <summary>
    ///     If an curator replies to users from discord, should it use their discord role color? (if applicable)
    ///     Overrides DiscordReplyColor and CuratorCwoinkColor.
    /// </summary>
    public static readonly CVarDef<bool> UseDiscordRoleColorInCwoinks =
        CVarDef.Create("admin.cwoink_use_discord_role_color", false, CVar.SERVERONLY);

    /// <summary>
    ///     If an curator replies to users from discord, should it use their discord role name? (if applicable)
    /// </summary>
    public static readonly CVarDef<bool> UseDiscordRoleNameInCwoinks =
        CVarDef.Create("admin.cwoink_use_discord_role_name", false, CVar.SERVERONLY);

    /// <summary>
    ///     The text before an curator's name when replying from discord to indicate they're speaking from discord.
    /// </summary>
    public static readonly CVarDef<string> DiscordCwoinkReplyPrefix =
        CVarDef.Create("admin.discord_cwoink_reply_prefix", "(DC) ", CVar.SERVERONLY);

    /// <summary>
    ///     The color of the names of curators. This is the fallback color for curators.
    /// </summary>
    public static readonly CVarDef<string> CuratorCwoinkColor =
        CVarDef.Create("admin.curator_cwoink_color", "#9552cc", CVar.SERVERONLY);

    /// <summary>
    ///     The color of the names of curators who reply from discord. Leave empty to disable.
    ///     Overrides CuratorCwoinkColor.
    /// </summary>
    public static readonly CVarDef<string> DiscordCwoinkReplyColor =
        CVarDef.Create("admin.discord_cwoink_reply_color", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Overrides the name the client sees in chelps. Set empty to disable.
    /// </summary>
    public static readonly CVarDef<string> CuratorChelpOverrideClientName =
        CVarDef.Create("admin.override_curatorname_in_client_chelp", string.Empty, CVar.SERVERONLY);
}
