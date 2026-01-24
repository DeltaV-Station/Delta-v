using Robust.Shared.Prototypes;

namespace Content.Shared.Shipyard.Prototypes;

/// <summary>
/// Like <c>TagPrototype</c> but for vessel categories.
/// Prevents making typos being silently ignored by the linter.
/// </summary>
[Prototype("vesselCategory")]
public sealed class VesselCategoryPrototype : IPrototype
{
    /// <summary>
    /// The unique ID for the vessel category.
    /// </summary>
    [ViewVariables, IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// The LocId containing the localization ID of the category name.
    /// </summary>
    [DataField(required: true)]
    private LocId Name { get; set; }

    /// <summary>
    /// Gets the localized string from the LocID set by <see cref="Name"/>
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);
}
