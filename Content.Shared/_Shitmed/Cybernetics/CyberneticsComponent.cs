using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Cybernetics;

/// <summary>
/// Component for cybernetic implants that can be installed in entities.
/// Causes EMPs to disable them temporarily.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CyberneticsComponent : Component;
