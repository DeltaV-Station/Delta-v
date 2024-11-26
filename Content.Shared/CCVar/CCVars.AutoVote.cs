using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
        /*
        * AUTOVOTE SYSTEM
        */

        /// Enables the automatic voting system.
        public static readonly CVarDef<bool> AutoVoteEnabled =
            CVarDef.Create("vote.autovote_enabled", true, CVar.SERVERONLY);

        /// Automatically starts a map vote when returning to the lobby.
        /// Requires auto voting to be enabled.  
        public static readonly CVarDef<bool> MapAutoVoteEnabled =
            CVarDef.Create("vote.map_autovote_enabled", true, CVar.SERVERONLY);

        /// Automatically starts a gamemode vote when returning to the lobby.
        /// Requires auto voting to be enabled.  
        public static readonly CVarDef<bool> PresetAutoVoteEnabled =
            CVarDef.Create("vote.preset_autovote_enabled", true, CVar.SERVERONLY);
}
