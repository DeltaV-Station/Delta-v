using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.ReverseEngineering;

/// <summary>
/// This item has some value in reverse engineering lathe recipes.
/// </summary>
[RegisterComponent]
public sealed partial class ReverseEngineeringComponent : Component
{
    /// <summary>
    /// The recipes that can be reverse engineered from this.
    /// Does not neccesarily line up with lathe recipes.
    /// </summary>
    [DataField("recipes")]
    public List<string>? Recipes;

    /// <summary>
    /// Difficulty score 1-5 how hard this is to reverse engineer.
    /// </summary>
    [DataField("difficulty")]
    public int Difficulty = 1;

    /// <summary>
    /// Used to mark whether this entity intentionally lets its children use its recipe.
    /// e.g. all jetpacks unlock the same jetpack recipe. Used for tests.
    /// </summary>
    [DataField("generic")]
    public bool Generic = false;

    /// <summary>
    /// A new item that should be given back by the reverse engineering machine instead of this one.
    /// E.g., NT aligned versions of syndicate items.
    /// </summary>
    [DataField("newItem", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? NewItem;

    /// <summary>
    /// A research disk that should be given by the reverse engineering machine when it completes something.
    /// Difficulty 1/2 = ResearchDisk, Difficulty 3/4 = ResearchDisk 5000, Difficulty 5/above = ResearchDisk 10000.
    /// Should I keep it yml? We shall see. More work with yml technicly.
    /// </summary>
    [DataField("research", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? Research;
}
