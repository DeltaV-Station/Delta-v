using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Fabricator;

[Prototype]
public sealed partial class FabricatorRecipeGroupPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;
}
