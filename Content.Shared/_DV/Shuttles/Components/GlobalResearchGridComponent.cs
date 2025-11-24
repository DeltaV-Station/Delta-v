using Robust.Shared.GameStates;

namespace Content.Shared._DV.Shuttles.Components;

/// <summary>
/// Makes a grid support research clients using all servers, even cross-map.
/// Useful for antag shuttles that need to make use of research.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GlobalResearchGridComponent : Component;
