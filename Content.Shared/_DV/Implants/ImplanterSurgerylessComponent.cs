using Robust.Shared.GameStates;

namespace Content.Shared._DV.Implants;

/// <summary>
///     Indicator that an implanter doesn't require surgery to work
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ImplanterSurgerylessComponent : Component;
