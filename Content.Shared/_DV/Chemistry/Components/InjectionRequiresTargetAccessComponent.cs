using Robust.Shared.GameStates;

namespace Content.Shared._DV.Chemistry.Components;

/// <summary>
/// The injector cannot inject into targets that do not satisfy the access requirements.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InjectionRequiresTargetAccessComponent : Component
{
    /// <summary>
    /// The popup shown when the target doesn't have the required access.
    /// </summary>
    [DataField]
    public LocId? PopupMessage = "injector-component-target-insufficent-access";
}
