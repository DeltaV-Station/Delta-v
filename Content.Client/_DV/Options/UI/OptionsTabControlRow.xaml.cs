using Content.Client._DV.Options.UI;
using Robust.Shared.Configuration;

namespace Content.Client.Options.UI;

public sealed partial class OptionsTabControlRow
{
    /// <summary>
    ///     Add a color slider option, backed by a simple string CVar.
    /// </summary>
    /// <param name="cVar">The CVar represented by the slider.</param>
    /// <param name="slider">The UI control for the option.</param>
    /// <returns>The option instance backing the added option.</returns>
    public OptionColorSliderCVar AddOptionColorSlider(
        CVarDef<string> cVar,
        OptionColorSlider slider)
    {
        return AddOption(new OptionColorSliderCVar(this, _cfg, cVar, slider));
    }
}
