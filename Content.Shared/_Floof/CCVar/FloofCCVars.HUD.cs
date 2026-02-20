using Robust.Shared.Configuration;

namespace Content.Shared._Floof.CCVar;

public sealed partial class FloofCCVars
{
    /// <summary>
    ///     Whether to display an item offering indicator above the player's cursor when in item offering mode.
    /// </summary>
    public static readonly CVarDef<bool> OfferModeIndicatorsPointShow =
        CVarDef.Create("hud.offer_mode_indicators_point_show", true, CVar.ARCHIVE | CVar.CLIENTONLY);
}
