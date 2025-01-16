using Robust.Shared.GameStates;

namespace Content.Shared._DV.Hologram;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedHologramSystem))]
public sealed partial class HologramComponent : Component
{
    /// <summary>
    /// To save the state of whatever the component was added to, in case it occludes when the component is added.
    /// </summary>
    [DataField("occludes")]
    public bool Occludes = false;

    /// <summary>
    /// Do we stop disarms or not?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("preventDisarm")]
    public bool PreventDisarm = false;
}
