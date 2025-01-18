using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traits.Assorted;

/// <summary>
/// This is used for the unborgable trait, which blacklists a brain from MMIs.
/// If this is added to a body, it gets moved to its brain if it has one.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UnborgableComponent : Component;
