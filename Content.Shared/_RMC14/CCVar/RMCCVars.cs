<<<<<<< HEAD
﻿using Robust.Shared;
=======
using Robust.Shared;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

[CVarDefs]
<<<<<<< HEAD
public sealed class RMCCVars : CVars
{
    public static readonly CVarDef<float> RMCMentorHelpRateLimitPeriod =
        CVarDef.Create("rmc.mentor_help_rate_limit_period", 2f, CVar.SERVERONLY);

    public static readonly CVarDef<int> RMCMentorHelpRateLimitCount =
        CVarDef.Create("rmc.mentor_help_rate_limit_count", 10, CVar.SERVERONLY);

    public static readonly CVarDef<string> RMCMentorHelpSound =
        CVarDef.Create("rmc.mentor_help_sound", "/Audio/_RMC14/Effects/Admin/mhelp.ogg", CVar.ARCHIVE | CVar.REPLICATED); // DeltaV - change from CVar.CLIENTONLY to CVar.REPLICATED
=======
public sealed partial class RMCCVars : CVars
{
    public static readonly CVarDef<float> RMCEmoteCooldownSeconds =
        CVarDef.Create("rmc.emote_cooldown_seconds", 3f, CVar.SERVER | CVar.REPLICATED);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
}
