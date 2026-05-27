// SPDX-FileCopyrightText: 2026 taydeo <tay@funkystation.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Configuration;

namespace Content.Shared._Funkystation.CCVars;

[CVarDefs]
public sealed class CCVars_Funky
{
    /// <summary>
    /// If the content warning should be displayed.
    /// </summary>
    public static readonly CVarDef<bool> ContentWarningDisplay =
        CVarDef.Create("cw.display", true, CVar.SERVER | CVar.REPLICATED);

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
