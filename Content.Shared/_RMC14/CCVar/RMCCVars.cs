using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

[CVarDefs]
public sealed partial class RMCCVars : CVars
{
    // DeltaV - Shows an icon above new players
    public static readonly CVarDef<bool> RMCShowNewPlayerIcons =
        CVarDef.Create("rmc.show_new_player_icons", true, CVar.REPLICATED | CVar.CLIENT | CVar.ARCHIVE);

    public static readonly CVarDef<int> RMCNewPlayerTimeTotalHours =
        CVarDef.Create("rmc.new_player_time_total_hours", 25, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> RMCNewPlayerTimeJobHours =
        CVarDef.Create("rmc.new_player_time_job_hours", 10, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> RMCBrandNewPlayerTimeJobHours =
        CVarDef.Create("rmc.brand_new_player_time_job_hours", 1, CVar.REPLICATED | CVar.SERVER);
}
