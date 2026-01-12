using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.SignLanguage.Prototypes;

/// <summary>
/// Sign language event - second menu in the sign language system.
/// Represents what is happening (contextual based on topic category).
/// </summary>
[Prototype]
public sealed class SignEventPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization string for the event name. Displayed in the radial UI.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Which topic categories this event applies to.
    /// If empty, applies to all categories.
    /// </summary>
    [DataField]
    public HashSet<SignTopicCategory> ApplicableCategories = new();

    /// <summary>
    /// An icon used to visually represent the event in radial UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Actions/sign.png"));

    /// <summary>
    /// Sort order for consistent layout.
    /// </summary>
    [DataField]
    public int Priority;
}
