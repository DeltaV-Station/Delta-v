using Robust.Shared.Prototypes;

namespace Content.Server._DV.Mapping;

/// <summary>
/// A mapping category that can be applied to items.
/// Maps can specify what categories they allow.
/// </summary>
[Prototype]
public sealed partial class MappingCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    /// <summary>
    /// Ignores checking items that are inside containers for this category.
    /// Useful for stamps which are not allowed to be mapped directly, but spawn in head lockers.
    /// </summary>
    [DataField]
    public bool IgnoreInsideContainer;
}
