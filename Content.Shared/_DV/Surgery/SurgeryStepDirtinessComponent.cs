using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Component that allows a step to increase tools and gloves' dirtiness
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryStepDirtinessComponent : Component
{
    /// <summary>
    ///     The amount of dirtiness this step should add to tools on completion
    /// </summary>
    [DataField]
    public FixedPoint2 ToolDirtiness = 2.0;

    /// <summary>
    ///     The amount of dirtiness this step should add to gloves on completion
    /// </summary>
    [DataField]
    public FixedPoint2 GloveDirtiness = 2.0;
}
