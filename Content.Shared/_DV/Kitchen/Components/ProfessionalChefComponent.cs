using Robust.Shared.GameStates;

namespace Content.Shared._DV.Kitchen.Components;

/// <summary>
/// Players with this component will never miss a throw into the deep fryer's basket.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProfessionalChefComponent : Component;
