using Robust.Shared.GameStates;

namespace Content.Shared._DV.Clothing.Components;

/// <summary>
///     Electrocutes players attempting to unequip clothes that have this component.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed partial class ShockOnUnequipComponent : Component
{
    /// <summary>
    /// The damage dealt to someone trying to unequip the item without insulation.
    /// </summary>
    [DataField]
    public int Damage = 5;

    /// <summary>
    /// The duration of the shock dealt to someone trying to unequip the item without insulation.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2);
}
