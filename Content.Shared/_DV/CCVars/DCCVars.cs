using Robust.Shared.Configuration;

namespace Content.Shared._DV.CCVars;

/// <summary>
/// DeltaV specific cvars.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming - Shush you
public sealed class DCCVars
{
    /*
     * Glimmer
     */

    /// <summary>
    ///    Whether glimmer is enabled.
    /// </summary>
    public static readonly CVarDef<bool> GlimmerEnabled =
        CVarDef.Create("glimmer.enabled", true, CVar.REPLICATED);

    /// <summary>
    ///     Passive glimmer drain per second.
    ///     Note that this is randomized and this is an average value.
    /// </summary>
    public static readonly CVarDef<float> GlimmerLostPerSecond =
        CVarDef.Create("glimmer.passive_drain_per_second", 0.1f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether random rolls for psionics are allowed.
    ///     Guaranteed psionics will still go through.
    /// </summary>
    public static readonly CVarDef<bool> PsionicRollsEnabled =
        CVarDef.Create("psionics.rolls_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// Anti-EORG measure. Will add pacified to all players upon round end.
    /// Its not perfect, but gets the job done.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndPacifist =
        CVarDef.Create("game.round_end_pacifist", false, CVar.SERVERONLY);

    /*
     * No EORG
     */

    /// <summary>
    /// Whether the no EORG popup is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndNoEorgPopup =
        CVarDef.Create("game.round_end_eorg_popup_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Skip the no EORG popup.
    /// </summary>
    public static readonly CVarDef<bool> SkipRoundEndNoEorgPopup =
        CVarDef.Create("game.skip_round_end_eorg_popup", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to display the EORG popup for.
    /// </summary>
    public static readonly CVarDef<float> RoundEndNoEorgPopupTime =
        CVarDef.Create("game.round_end_eorg_popup_time", 5f, CVar.SERVER | CVar.REPLICATED);

    /*
     * Auto ACO
     */

    /// <summary>
    /// How long with no captain before requesting an ACO be elected.
    /// </summary>
    public static readonly CVarDef<float> RequestAcoDelay =
        CVarDef.Create("game.request_aco_delay_minutes", 15f, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Determines whether an ACO should be requested when the captain leaves during the round,
    /// in addition to cases where there are no captains at round start.
    /// </summary>
    public static readonly CVarDef<bool> RequestAcoOnCaptainDeparture =
        CVarDef.Create("game.request_aco_on_captain_departure", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Determines whether All Access (AA) should be automatically unlocked if no captain is present.
    /// </summary>
    public static readonly CVarDef<bool> AutoUnlockAllAccessEnabled =
        CVarDef.Create("game.auto_unlock_aa_enabled", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long after an ACO request announcement is made before All Access (AA) should be unlocked.
    /// </summary>
    public static readonly CVarDef<float> AutoUnlockAllAccessDelay =
        CVarDef.Create("game.auto_unlock_aa_delay_minutes", 5f, CVar.SERVERONLY | CVar.ARCHIVE);

    /*
     * Misc.
     */

    /// <summary>
    /// Disables all vision filters for species like Vulpkanin or Harpies. There are good reasons someone might want to disable these.
    /// </summary>
    public static readonly CVarDef<bool> NoVisionFilters =
        CVarDef.Create("accessibility.no_vision_filters", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Whether the Shipyard is enabled.
    /// </summary>
    public static readonly CVarDef<bool> Shipyard =
        CVarDef.Create("shuttle.shipyard", true, CVar.SERVERONLY);

    /*
     * Feedback webhook
     */

    /// <summary>
    ///     Discord webhook URL for getting feedback from players. If empty, will not relay the feedback.
    /// </summary>
    public static readonly CVarDef<string> DiscordPlayerFeedbackWebhook =
        CVarDef.Create("discord.player_feedback_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     Use the admin's Admin OOC color in bwoinks.
    ///     If either the ooc color or this is not set, uses the admin.admin_bwoink_color value.
    /// </summary>
    public static readonly CVarDef<bool> UseAdminOOCColorInBwoinks =
        CVarDef.Create("admin.bwoink_use_admin_ooc_color", false, CVar.SERVERONLY);

    /// <summary>
    ///     If an admin replies to users from discord, should it use their discord role color? (if applicable)
    ///     Overrides DiscordReplyColor and AdminBwoinkColor.
    /// </summary>
    public static readonly CVarDef<bool> UseDiscordRoleColor =
        CVarDef.Create("admin.use_discord_role_color", false, CVar.SERVERONLY);

    /// <summary>
    ///     If an admin replies to users from discord, should it use their discord role name? (if applicable)
    /// </summary>
    public static readonly CVarDef<bool> UseDiscordRoleName =
        CVarDef.Create("admin.use_discord_role_name", false, CVar.SERVERONLY);

    /// <summary>
    ///     The text before an admin's name when replying from discord to indicate they're speaking from discord.
    /// </summary>
    public static readonly CVarDef<string> DiscordReplyPrefix =
        CVarDef.Create("admin.discord_reply_prefix", "(DC) ", CVar.SERVERONLY);

    /// <summary>
    ///     The color of the names of admins. This is the fallback color for admins.
    /// </summary>
    public static readonly CVarDef<string> AdminBwoinkColor =
        CVarDef.Create("admin.admin_bwoink_color", "red", CVar.SERVERONLY);

    /// <summary>
    ///     The color of the names of admins who reply from discord. Leave empty to disable.
    ///     Overrides AdminBwoinkColor.
    /// </summary>
    public static readonly CVarDef<string> DiscordReplyColor =
        CVarDef.Create("admin.discord_reply_color", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///    Whether or not to disable the preset selecting test rule from running. Should be disabled in production. DeltaV specific, attached to Impstation Secret concurrent feature.
    /// </summary>
    public static readonly CVarDef<bool> EnableBacktoBack =
        CVarDef.Create("game.disable_preset_test", false, CVar.SERVERONLY);

    /// <summary>
    /// A string containing a list of newline-separated strings to be highlighted in the chat.
    /// </summary>
    public static readonly CVarDef<string> ChatHighlights =
        CVarDef.Create("deltav.chat.highlights",
            "",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "A list of newline-separated strings to be highlighted in the chat.");

    /// <summary>
    /// An option to toggle the automatic filling of the highlights with the character's info, if available.
    /// </summary>
    public static readonly CVarDef<bool> ChatAutoFillHighlights =
        CVarDef.Create("deltav.chat.auto_fill_highlights",
            false,
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "Toggles automatically filling the highlights with the character's information.");

    /// <summary>
    /// The color in which the highlights will be displayed.
    /// </summary>
    public static readonly CVarDef<string> ChatHighlightsColor =
        CVarDef.Create("deltav.chat.highlights_color",
            "#17FFC1FF",
            CVar.CLIENTONLY | CVar.ARCHIVE,
            "The color in which the highlights will be displayed.");

    /* Laying down combat */

    /// <summary>
    /// Modifier to apply to all melee attacks when laying down.
    /// Don't increase this above 1...
    /// </summary>
    public static readonly CVarDef<float> LayingDownMeleeMod =
        CVarDef.Create("game.laying_down_melee_mod", 0.25f, CVar.REPLICATED);

    /// <summary>
    ///    Maximum number of characters in objective summaries.
    /// </summary>
    public static readonly CVarDef<int> MaxObjectiveSummaryLength =
        CVarDef.Create("game.max_objective_summary_length", 256, CVar.SERVER | CVar.REPLICATED);
}
