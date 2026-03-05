using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits;

/// <summary>
/// Prototype for a category of traits.
/// Categories organize traits and can impose their own limits.
/// </summary>
[Prototype]
public sealed partial class TraitCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization key for the category's display name.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Display order priority. Lower values appear first.
    /// </summary>
    [DataField]
    public int Priority;

    /// <summary>
    /// Maximum number of traits that can be selected from this category.
    /// Null means unlimited (only global limit applies).
    /// </summary>
    [DataField]
    public int? MaxTraits;

    /// <summary>
    /// Maximum trait points that can be spent in this category.
    /// Null means unlimited (only global limit applies).
    /// </summary>
    [DataField]
    public int? MaxPoints;

    /// <summary>
    /// Color hex for the category header accent.
    /// </summary>
    [DataField]
    public Color AccentColor = Color.FromHex("#4a9eff");

    /// <summary>
    /// Whether this category starts expanded or collapsed.
    /// </summary>
    [DataField]
    public bool DefaultExpanded = true;
}
