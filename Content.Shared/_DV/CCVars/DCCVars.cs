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
    /// How long after the announcement before the spare ID is unlocked
    /// </summary>
    public static readonly CVarDef<TimeSpan> SpareIdUnlockDelay =
        CVarDef.Create("game.spare_id.unlock_delay", TimeSpan.FromMinutes(5), CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to wait before checking for a captain after roundstart
    /// </summary>
    public static readonly CVarDef<TimeSpan> SpareIdAlertDelay =
        CVarDef.Create("game.spare_id.alert_delay", TimeSpan.FromMinutes(15), CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Determines if the automatic spare ID process should automatically unlock the cabinet
    /// </summary>
    public static readonly CVarDef<bool> SpareIdAutoUnlock =
        CVarDef.Create("game.spare_id.auto_unlock", true, CVar.SERVERONLY | CVar.ARCHIVE);

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

    /* Chat highlighting */

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

    /* Traitors */

    /// <summary>
    /// Base ransom for a non-humanoid mob, like shiva.
    /// </summary>
    public static readonly CVarDef<float> MobRansom =
        CVarDef.Create("game.ransom.mob_base", 5000f, CVar.REPLICATED);

    /// <summary>
    /// Base ransom for a humanoid.
    /// </summary>
    public static readonly CVarDef<float> HumanoidRansom =
        CVarDef.Create("game.ransom.humanoid_base", 10000f, CVar.REPLICATED);

    /// <summary>
    /// Ransom modifier for critical mobs.
    /// </summary>
    public static readonly CVarDef<float> RansomCritModifier =
        CVarDef.Create("game.ransom.critical_modifier", 0.5f, CVar.REPLICATED);

    /// <summary>
    /// Ransom modifier for dead mobs.
    /// The ransomer will also fail their objective.
    /// </summary>
    public static readonly CVarDef<float> RansomDeadModifier =
        CVarDef.Create("game.ransom.dead_modifier", 0.2f, CVar.REPLICATED);

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

    /* OOC shuttle vote */

    /// <summary>
    /// How long players should have to vote on the round end shuttle being sent
    /// </summary>
    public static readonly CVarDef<TimeSpan> EmergencyShuttleVoteTime =
        CVarDef.Create("shuttle.vote_time", TimeSpan.FromMinutes(1), CVar.SERVER);

    /*
     * Cosmic Cult
     */
    /// <summary>
    /// How much entropy a convert is worth towards the next monument tier.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultistEntropyValue =
        CVarDef.Create("cosmiccult.cultist_entropy_value", 7, CVar.SERVER);

    /// <summary>
    /// How much of the crew the cult is aiming to convert for a tier 3 monument.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultTargetConversionPercent =
        CVarDef.Create("cosmiccult.target_conversion_percent", 40, CVar.SERVER);

    /// <summary>
    /// How long the timer for the cult's stewardship vote lasts.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultStewardVoteTimer =
        CVarDef.Create("cosmiccult.steward_vote_timer", 40, CVar.SERVER);

    /// <summary>
    /// The delay between the monument getting upgraded to tier 2 and the crew learning of that fact. the monument cannot be upgraded again in this time.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultT2RevealDelaySeconds =
        CVarDef.Create("cosmiccult.t2_reveal_delay_seconds", 120, CVar.SERVER);

    /// <summary>
    /// The delay between the monument getting upgraded to tier 3 and the crew learning of that fact. the monument cannot be upgraded again in this time.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultT3RevealDelaySeconds =
        CVarDef.Create("cosmiccult.t3_reveal_delay_seconds", 60, CVar.SERVER);

    /// <summary>
    /// The delay between the monument getting upgraded to tier 3 and the finale starting.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultFinaleDelaySeconds =
        CVarDef.Create("cosmiccult.extra_entropy_for_finale", 150, CVar.SERVER);
}
