using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Component that allows an entity to be cross contamined from being used in surgery
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SurgeryCleanSystem))]
[AutoGenerateComponentState]
public sealed partial class SurgeryCrossContaminationComponent : Component
{
    /// <summary>
    ///     Patient DNAs that are present on this dirtied tool
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<string> DNAs = new();
}
