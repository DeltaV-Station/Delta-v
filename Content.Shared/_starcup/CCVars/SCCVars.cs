using Robust.Shared.Configuration;

namespace Content.Shared._starcup.CCVars;

[CVarDefs]
public sealed partial class SCCVars
{
    /// <summary>
    /// Sets the minimum amount of solution a puddle needs before it can create footsteps
    /// </summary>
    public static readonly CVarDef<float> MinimumPuddleSizeForFootprints = CVarDef.Create("footprints.minimum_puddle_size", 6f, CVar.SERVERONLY);
}
