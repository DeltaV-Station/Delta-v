using Content.Shared._EE.FootPrint.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._EE.FootPrint;

/// <summary>
/// Component attached to grids to track footprints per tile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(FootPrintsSystem))]
public sealed partial class GridFootPrintsComponent : Component
{
    /// <summary>
    /// Tracks all footprints on this grid, organized by tile position.
    /// NOTE: Not auto-networked - we use custom delta states instead
    /// </summary>
    [DataField]
    public Dictionary<Vector2i, List<NetEntity>> FootPrintsByTile = new();

    /// <summary>
    /// Total count of all footprints on this grid.
    /// </summary>
    [DataField]
    public int TotalFootPrints;

    /// <summary>
    /// Tracks which tiles have been modified since the last state send.
    /// </summary>
    [ViewVariables]
    public HashSet<Vector2i> DirtyTiles = new();

    /// <summary>
    /// Tracks which tiles have been completely removed since the last state send.
    /// </summary>
    [ViewVariables]
    public HashSet<Vector2i> RemovedTiles = new();

    /// <summary>
    /// If true, the next state will be a full state instead of delta.
    /// </summary>
    [ViewVariables]
    public bool NeedFullState = true;
}

/// <summary>
/// Custom component state that supports delta compression.
/// Only sends changed tiles instead of the entire dictionary.
/// </summary>
[Serializable, NetSerializable]
public sealed class GridFootPrintsComponentState(
    bool fullState,
    Dictionary<Vector2i, List<NetEntity>>? modified,
    HashSet<Vector2i>? removed,
    int totalFootPrints)
    : IComponentState
{
    /// <summary>
    /// If true, this is a full state sync. Client should replace entire dictionary.
    /// If false, this is a delta - client should apply changes.
    /// </summary>
    public bool FullState = fullState;

    /// <summary>
    /// Modified or added tiles and their footprint lists.
    /// For full state: contains all tiles.
    /// For delta: contains only changed tiles.
    /// </summary>
    public Dictionary<Vector2i, List<NetEntity>>? Modified = modified;

    /// <summary>
    /// Tiles that have been completely cleared (no footprints remaining).
    /// Only relevant for delta states.
    /// </summary>
    public HashSet<Vector2i>? Removed = removed;

    /// <summary>
    /// Total count of footprints on the grid.
    /// </summary>
    public int TotalFootPrints = totalFootPrints;
}
