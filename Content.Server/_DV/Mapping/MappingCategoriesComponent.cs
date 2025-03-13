using Robust.Shared.Prototypes;

namespace Content.Server._DV.Mapping;

/// <summary>
/// Component added to prototypes to prevent them being mapped on stations that do not allow them.
/// </summary>
[RegisterComponent, Access(typeof(MappingCategoriesSystem))]
public sealed partial class MappingCategoriesComponent : Component
{
    /// <summary>
    /// The categories this prototype has.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<MappingCategoryPrototype>> Categories = new();
}
