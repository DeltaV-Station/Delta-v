using Robust.Shared.GameStates;

namespace Content.Shared._DV.Shuttles.Components;

/// <summary>
/// Marker component for the mining shuttle grid.
/// Used for lavaland's FTL whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MiningShuttleComponent : Component;
