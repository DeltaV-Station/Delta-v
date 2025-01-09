using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._Impstation.CCVar;

/// <summary>
/// Contains all the CVars used by the random announcer system. From Impstation. 
// I'm pretty sure this is the entirety of what it needs to function.
/// </summary>

// ReSharper disable once InconsistentNaming - Shush you!!
[CVarDefs]
public sealed class ImpCCVars
{
        /*
        * Announcers
        */

    /// <summary>
    ///     Weighted list of announcers to choose from
    /// </summary>
    public static readonly CVarDef<string> AnnouncerList =
        CVarDef.Create("announcer.list", "RandomAnnouncers", CVar.REPLICATED);

    /// <summary>
    ///     Optionally force set an announcer
    /// </summary>
    public static readonly CVarDef<string> Announcer =
        CVarDef.Create("announcer.announcer", "", CVar.SERVERONLY);

    /// <summary>
    ///     Optionally blacklist announcers
    ///     List of IDs separated by commas
    /// </summary>
    public static readonly CVarDef<string> AnnouncerBlacklist =
        CVarDef.Create("announcer.blacklist", "", CVar.SERVERONLY);

    /// <summary>
    ///     Changes how loud the announcers are for the client
    /// </summary>
    public static readonly CVarDef<float> AnnouncerVolume =
        CVarDef.Create("announcer.volume", 0.5f, CVar.ARCHIVE | CVar.CLIENTONLY);

    /// <summary>
    ///     Disables multiple announcement sounds from playing at once
    /// </summary>
    public static readonly CVarDef<bool> AnnouncerDisableMultipleSounds =
        CVarDef.Create("announcer.disable_multiple_sounds", false, CVar.ARCHIVE | CVar.CLIENTONLY);

}