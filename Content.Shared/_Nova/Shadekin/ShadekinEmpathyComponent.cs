using Robust.Shared.GameStates;

namespace Content.Shared._Nova.Shadekin;

/// <summary>
/// Marks an entity as capable of sending and receiving Shadekin empathic communication.
/// Shadekin with this component can use empathic speech that nearby Shadekin can hear.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadekinEmpathyComponent : Component
{
    /// <summary>
    /// Range in tiles that empathic messages can be received.
    /// </summary>
    [DataField]
    public float Range = 15f;
}
