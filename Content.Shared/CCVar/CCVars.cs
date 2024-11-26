using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

/// <summary>
/// Contains all the CVars used by content.
/// </summary>
/// <remarks>
/// NOTICE FOR FORKS: Put your own CVars in a separate file with a different [CVarDefs] attribute. RT will automatically pick up on it.
/// </remarks>
[CVarDefs]
public sealed partial class CCVars : CVars
{
    // Only debug stuff lives here.

    /// <summary>
    /// A simple toggle to test <c>OptionsVisualizerComponent</c>.
    /// </summary>
    public static readonly CVarDef<bool> DebugOptionVisualizerTest =
        CVarDef.Create("debug.option_visualizer_test", false, CVar.CLIENTONLY);

    /// <summary>
    /// Set to true to disable parallel processing in the pow3r solver.
    /// </summary>
    public static readonly CVarDef<bool> DebugPow3rDisableParallel =
        CVarDef.Create("debug.pow3r_disable_parallel", true, CVar.SERVERONLY);

     /// <summary>
     /// Toggle for non-gameplay-affecting or otherwise status indicative post-process effects, such additive lighting.
     /// In the future, this could probably be turned into an enum that allows only disabling more expensive post-process shaders.
     /// However, for now (mid-July of 2024), this only applies specifically to a particularly cheap shader: additive lighting.
     /// </summary>
     public static readonly CVarDef<bool> PostProcess =
         CVarDef.Create("graphics.post_process", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}