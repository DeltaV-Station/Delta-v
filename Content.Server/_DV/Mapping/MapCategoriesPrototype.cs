using Robust.Shared.Prototypes;

namespace Content.Server._DV.Mapping;

/// <summary>
/// Defines what entity <see cref="MappingCategoryPrototype"/> can be added to a certain map.
/// </summary>
[Prototype]
public sealed partial class MapCategoriesPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    /// <summary>
    /// The map path to apply these categories for.
    /// </summary>
    [DataField(required: true)]
    public string Map = string.Empty;

    /// <summary>
    /// The categories that are allowed for the defined map.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<MappingCategoryPrototype>> Allowed = new();
}
