using Content.Shared._EE.FootPrint.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._EE.FootPrint;

/// <summary>
/// Component attached to grids to track footprints per tile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(FootPrintsSystem))]
[AutoGenerateComponentState]
public sealed partial class GridFootPrintsComponent : Component
{
    /// <summary>
    /// Tracks all footprints on this grid, organized by tile position.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Vector2i, List<NetEntity>> FootPrintsByTile = new();

    /// <summary>
    /// Total count of all footprints on this grid.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalFootPrints;
}
