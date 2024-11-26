using Robust.Shared.Configuration;

namespace Content.Shared.DeltaV.CCVars;

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
}
