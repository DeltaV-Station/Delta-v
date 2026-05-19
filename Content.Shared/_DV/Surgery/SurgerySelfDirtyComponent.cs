using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Marker component that indicates that the entity should become dirtied instead of its tools doing surgery
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgerySelfDirtyComponent : Component;
