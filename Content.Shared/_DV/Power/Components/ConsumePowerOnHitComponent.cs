using Robust.Shared.GameStates;

namespace Content.Shared._DV.Power.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConsumePowerOnHitComponent : Component
{
    /// <summary>
    /// How much charge will be spent with each hit.
    /// </summary>
    [DataField]
    public float ChargePerHit = 50f;
}
