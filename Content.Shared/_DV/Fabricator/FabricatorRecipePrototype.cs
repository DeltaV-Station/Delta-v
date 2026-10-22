using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Materials;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Fabricator;

[Prototype]
public sealed partial class FabricatorRecipePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("name")]
    private string _name = string.Empty;

    [DataField]
    public string Group { get; private set; } = string.Empty;

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents { get; private set; } = new();

    [DataField]
    public Dictionary<ProtoId<EntityPrototype>, int> Solids { get; private set; } = new();

    [DataField]
    public Dictionary<ProtoId<MaterialPrototype>, int> Materials { get; private set; } = new();

    [DataField]
    public TimeSpan CompletionTime = TimeSpan.FromSeconds(5);

    [DataField]
    public ProtoId<EntityPrototype> Result { get; private set; } = default!;

    public string Name => Loc.GetString(_name);

    [DataField]
    public List<ProtoId<FabricatorRecipeGroupPrototype>> RecipeGroups = new();

    /// <summary>
    ///    Count the number of ingredients in a recipe for sorting the recipe list.
    ///    This makes sure that where ingredient lists overlap, the more complex
    ///    recipe is picked first.
    /// </summary>
    public FixedPoint2 IngredientCount => Reagents.Count + Solids.Count;


    public sealed class FabricatorRecipeComparer : Comparer<FabricatorRecipePrototype>
    {
        public override int Compare(FabricatorRecipePrototype? x, FabricatorRecipePrototype? y)
        {
            if (x == null || y == null)
                return 0;
            return -x.IngredientCount.CompareTo(y.IngredientCount);
        }
    }
}
