using Robust.Shared.GameStates;

namespace Content.Shared._DV.Clothing.Components;

/// <summary>
///     Electrocutes players attempting to unequip clothes that have this component.
///     Use AccessReaderComponent to stop shocking characters with certain access.
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

    /// <summary>
    /// If true, only shock unequipping entity if lacking access specified in AccessReader component.
    /// </summary>
    [DataField]
    public bool UseAccess = true;
}
