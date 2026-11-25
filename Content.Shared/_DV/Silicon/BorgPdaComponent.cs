using Robust.Shared.GameStates;

namespace Content.Shared._DV.Silicon;

/// <summary>
/// Component that indicates a built-in PDA on a cyborg that can be accessed by the AI
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BorgPdaComponent : Component;
