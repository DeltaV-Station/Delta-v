using Robust.Shared.GameStates;

namespace Content.Shared._DV.Chemistry.Components;

/// <summary>
/// Prevents syringes being used on this entity.
/// Hyposprays are unaffected.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BlockInjectionComponent : Component
{
    [DataField]
    public LocId ReasonLocId = "injector-component-deny-user-chitnid";
}
