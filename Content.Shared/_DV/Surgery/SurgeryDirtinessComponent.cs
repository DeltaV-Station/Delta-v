using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Component that allows an entity to take on dirtiness from being used in surgery
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SurgeryCleanSystem))]
[AutoGenerateComponentState]
public sealed partial class SurgeryDirtinessComponent : Component
{
    /// <summary>
    ///     The level of dirtiness this component represents; above 50 is usually where consequences start to happen
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Dirtiness = 0.0;
}
