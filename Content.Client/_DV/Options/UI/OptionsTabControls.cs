using Content.Client.Options.UI;
using Robust.Shared.Configuration;

namespace Content.Client._DV.Options.UI;

/// <summary>
/// Implementation of a CVar option that simply corresponds with a string <see cref="OptionColorSlider"/>.
/// </summary>
/// <seealso cref="OptionsTabControlRow"/>
public sealed class OptionColorSliderCVar : BaseOptionCVar<string>
{
    private readonly OptionColorSlider _slider;

    protected override string Value
    {
        get => _slider.Slider.Color.ToHex();
        set
        {
            _slider.Slider.Color = Color.FromHex(value);
            UpdateLabelColor();
        }
    }

    /// <summary>
    /// Creates a new instance of this type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is generally more convenient to call overloads on <see cref="OptionsTabControlRow"/>
    /// such as <see cref="OptionsTabControlRow.AddOptionPercentSlider"/> instead of instantiating this type directly.
    /// </para>
    /// </remarks>
    /// <param name="controller">The control row that owns this option.</param>
    /// <param name="cfg">The configuration manager to get and set values from.</param>
    /// <param name="cVar">The CVar that is being controlled by this option.</param>
    /// <param name="slider">The UI control for the option.</param>
    public OptionColorSliderCVar(
        OptionsTabControlRow controller,
        IConfigurationManager cfg,
        CVarDef<string> cVar,
        OptionColorSlider slider) : base(controller, cfg, cVar)
    {
        _slider = slider;

        slider.Slider.OnColorChanged += _ =>
        {
            ValueChanged();
            UpdateLabelColor();
        };
    }

    private void UpdateLabelColor()
    {
        _slider.ExampleLabel.FontColorOverride = Color.FromHex(Value);
    }
}
