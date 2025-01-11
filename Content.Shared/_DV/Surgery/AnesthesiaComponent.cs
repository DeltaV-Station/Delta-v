using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Exists for use as a status effect. Allows surgical operations to not cause immense pain.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AnesthesiaComponent : Component;
