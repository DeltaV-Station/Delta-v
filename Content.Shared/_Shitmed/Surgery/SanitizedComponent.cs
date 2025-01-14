using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery;

/// <summary>
///     Prevents the entity from causing toxin damage to entities it does surgery on.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SanitizedComponent : Component { }
