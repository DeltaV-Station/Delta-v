using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// Anyone with this component can do CPR to people in critical state.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CanDoCPRComponent : Component;
