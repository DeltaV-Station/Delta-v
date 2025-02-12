namespace Content.Server._DV.Surgery;

/// <summary>
///     Component that allows an entity to be cross contamined from being used in surgery
/// </summary>
[RegisterComponent]
public sealed partial class SurgeryCrossContaminationComponent : Component
{
    /// <summary>
    ///     Patient DNAs that are present on this dirtied tool
    /// </summary>
    [DataField]
    public HashSet<string> DNAs = new();
}
