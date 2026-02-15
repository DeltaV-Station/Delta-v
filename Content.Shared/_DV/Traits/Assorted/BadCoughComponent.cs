using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// This is used for the persistent cough trait, anything with this component will cough occasionally if able.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class BadCoughComponent : Component;
