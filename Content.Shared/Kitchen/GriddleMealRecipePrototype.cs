using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Kitchen
{
    /// <summary>
    ///    A recipe for griddles.
    /// </summary>
    [Prototype("griddleMealRecipe")]
    public sealed partial class FoodRecipePrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        private string _name = string.Empty;

        [DataField("reagents", customTypeSerializer:typeof(PrototypeIdDictionarySerializer<FixedPoint2, ReagentPrototype>))]
        private Dictionary<string, FixedPoint2> _ingsReagents = new();

        [DataField("solids", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<FixedPoint2, EntityPrototype>))]
        private Dictionary<string, FixedPoint2> _ingsSolids = new ();

        [DataField("result", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string Result { get; private set; } = string.Empty;

        [DataField("time")]
        public uint CookTime { get; private set; } = 3;

        public string Name => Loc.GetString(_name);

        // Too lazy to change this into ReagentQuant.
        public IReadOnlyDictionary<string, FixedPoint2> IngredientsReagents => _ingsReagents;
        public IReadOnlyDictionary<string, FixedPoint2> IngredientsSolids => _ingsSolids;

        /// <summary>
        ///    Counts the number of ingredients in a recipe for sorting the recipe list.
        ///    This makes sure that where ingredient lists overlap, the more complex
        ///    recipe is picked first. (Like Microwaves)
        /// </summary>
        public FixedPoint2 IngredientCount()
        {
            FixedPoint2 n = 0;
            n += _ingsReagents.Count; // number of individual reagents
            foreach (FixedPoint2 i in _ingsSolids.Values) // sum of solid ingredients
            {
                n += i;
            }
            return n;
        }
    }
}
