using Robust.Shared.Configuration;

namespace Content.Shared._NF.CCVar;

[CVarDefs]
public sealed class NFCCVars
{
    /*
     *  Respawn
    */
    /// <summary>
    /// Whether or not respawning is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RespawnEnabled =
        CVarDef.Create("nf14.respawn.enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after going into cryosleep. Should be small, misclicks happen.
    /// </summary>
    public static readonly CVarDef<float> RespawnCryoFirstTime =
        CVarDef.Create("nf14.respawn.cryo_first_time", 20.0f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after death, or on subsequent cryo attempts.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("nf14.respawn.time", 1200.0f, CVar.SERVER | CVar.REPLICATED);
}
