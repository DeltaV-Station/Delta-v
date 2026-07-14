using System.Numerics;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Holds SS14 eye data not relevant for engine, e.g. lerp targets.
/// </summary>
// ES START
// STOP adding ACCESS to shit for NO REASON
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
// ES END
public sealed partial class ContentEyeComponent : Component
{
    /// <summary>
    /// Zoom we're lerping to.
    /// </summary>
    [DataField("targetZoom"), AutoNetworkedField]
    public Vector2 TargetZoom = Vector2.One;

    /// <summary>
    /// How far we're allowed to zoom out.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxZoom"), AutoNetworkedField]
    public Vector2 MaxZoom = Vector2.One;

    // ES START
    // uhh fuckk
    /// <summary>
    ///     Base rotation of this eye, because of grid movement/turning/etc. Modified by eye lerp.
    /// </summary>
    [DataField]
    public Angle BaseRotation = Angle.Zero;
    // ES END
}
