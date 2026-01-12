using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.SignLanguage.Prototypes;

/// <summary>
/// Sign language intensity modifier - optional fourth menu.
/// Affects how the sign is performed and displayed.
/// </summary>
[Prototype]
public sealed class SignIntensityPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization string for the intensity name. Displayed in the radial UI.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Localization string for how non-fluent observers see this intensity.
    /// For example: "sharp, urgent" or "slow, deliberate"
    /// </summary>
    [DataField(required: true)]
    public LocId NonFluentDescription;

    /// <summary>
    /// An icon used to visually represent the intensity in radial UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Actions/sign.png"));

    /// <summary>
    /// Text formatting to apply when this intensity is used.
    /// For example: "!!!" for URGENT, "..." for CALM
    /// </summary>
    [DataField]
    public string TextFormatting = "";

    /// <summary>
    /// Whether the signed message should be displayed in bold.
    /// Useful for urgent or panicked intensities.
    /// </summary>
    [DataField]
    public bool BoldMessage;

    /// <summary>
    /// Whether this is the default intensity (used when intensity menu is skipped).
    /// </summary>
    [DataField]
    public bool IsDefault;

    /// <summary>
    /// Sort order for consistent layout.
    /// </summary>
    [DataField]
    public int Priority;
}
