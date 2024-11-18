using Robust.Shared.Configuration;

namespace Content.Shared.Corvax.CCCVars;

[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class CCCVars
{
    /*
     * Station Goals
     */

    /// <summary>
    ///     Enables station goals
    /// </summary>
    public static readonly CVarDef<bool> StationGoalsEnabled =
        CVarDef.Create("game.station_goals", true, CVar.SERVERONLY);
}
