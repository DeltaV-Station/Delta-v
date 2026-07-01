using Robust.Shared.Configuration;

namespace Content.Shared._Funkystation.CCVars;

[CVarDefs]
public sealed class CCVars_Funky
{
    // imp start. Automatically disable content warnings in debug builds. // DeltaV - and tools, too
#if DEBUG || TOOLS
    private static bool _cwOnBuildType;
#else
    private static bool _cwOnBuildType = true;
#endif
    // imp end

    /// <summary>
    /// If the content warning should be displayed.
    /// </summary>
    public static readonly CVarDef<bool> ContentWarningDisplay =
        CVarDef.Create("cw.display", _cwOnBuildType, CVar.SERVER | CVar.REPLICATED); // imp. Automatically disable content warnings in debug builds.

    /// <summary>
    /// If ignoring the content warning should kick you from the server.
    /// </summary>
    public static readonly CVarDef<bool> ContentWarningKickOnIgnore =
        CVarDef.Create("cw.kick", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// If the content warning popup was acknowledged.
    /// </summary>
    public static readonly CVarDef<bool> ContentWarningAcknowledged =
        CVarDef.Create("cw.acknowledged", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
