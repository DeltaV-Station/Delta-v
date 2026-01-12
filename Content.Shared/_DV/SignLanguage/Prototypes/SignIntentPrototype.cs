using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.SignLanguage.Prototypes;

/// <summary>
/// Sign language intent - third menu in the sign language system.
/// Represents what the signer wants (mostly universal).
/// </summary>
[Prototype]
public sealed class SignIntentPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization string for the intent name. Displayed in the radial UI.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// An icon used to visually represent the intent in radial UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Actions/sign.png"));

    /// <summary>
    /// Sort order for consistent layout.
    /// </summary>
    [DataField]
    public int Priority;
}
