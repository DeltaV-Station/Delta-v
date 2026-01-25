using Content.Shared.FixedPoint;
using Content.Shared.Kitchen;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Kitchen;

/// <summary>
/// The base recipe for the deep fryer.
/// </summary>
/// <remarks>
/// This only handles the deep-fryer related stuff like oil consumption and burning result,
/// while the <see cref="FoodRecipePrototype"/> handles the recipe and results themselves.
/// This is done because while FoodRecipe is not generic and hardcoded to microwaves,
/// we can still reuse it for guidebook generation without duping the code.
/// </remarks>
[Prototype]
public sealed class DeepFryerRecipePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The <see cref="FoodRecipePrototype"/> to inherit from
    /// </summary>
    [DataField(required: true)]
    public ProtoId<FoodRecipePrototype> BaseRecipe;

    /// <summary>
    /// How long will it take for this food to burn once it finishes cooking?
    /// </summary>
    [DataField]
    public TimeSpan BurnTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How many units of oil will this recipe use up?
    /// </summary>
    [DataField]
    public FixedPoint2 OilConsumption = 2;

    /// <summary>
    /// The <see cref="EntityPrototype"/> this recipe will spawn when it finishes burning.
    /// </summary>
    [DataField]
    public EntProtoId BurnedResult = "FoodBadRecipe";
}
