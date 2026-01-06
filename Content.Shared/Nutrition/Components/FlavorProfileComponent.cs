using Robust.Shared.GameStates;

namespace Content.Shared.Nutrition.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState] // DV - Deep Fryers
public sealed partial class FlavorProfileComponent : Component
{
    /// <summary>
    ///     Localized string containing the base flavor of this entity.
    /// </summary>
    [DataField]
    [AutoNetworkedField] // DV - Deep Fryers
    public HashSet<string> Flavors = new(); // DV remove setter

    /// <summary>
    ///     Reagent IDs to ignore when processing this flavor profile. Defaults to nutriment.
    /// </summary>
    [DataField]
    public HashSet<string> IgnoreReagents { get; private set; } = new()
    {
        "Nutriment",
        "Vitamin",
        "Protein",
    };
}
