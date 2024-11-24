using Robust.Shared.Configuration;

namespace Content.Shared.White;

[CVarDefs]
public sealed class WhiteCVars
{
    #region GhostRespawn
    public static readonly CVarDef<double> GhostRespawnTime =
        CVarDef.Create("ghost.respawn_time", 15d, CVar.SERVERONLY);

    public static readonly CVarDef<int> GhostRespawnMaxPlayers =
        CVarDef.Create("ghost.respawn_max_players", 35, CVar.SERVERONLY);

    #endregion
}
