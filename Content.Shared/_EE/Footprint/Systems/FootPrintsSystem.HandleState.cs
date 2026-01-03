using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._EE.FootPrint.Systems;

public sealed partial class FootPrintsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private void InitializeStateHandling()
    {
        SubscribeLocalEvent<GridFootPrintsComponent, ComponentGetState>(OnGridFootPrintsGetState);
        SubscribeLocalEvent<GridFootPrintsComponent, ComponentHandleState>(OnGridFootPrintsHandleState);
    }

    private void OnGridFootPrintsGetState(Entity<GridFootPrintsComponent> ent, ref ComponentGetState args)
    {
        // Determine if we should send full state or delta
        // Send full state if:
        // - Component explicitly needs it
        // - This is the first send to this player (FromTick is very old)
        // - Periodically for desync recovery
        var sendFullState = ent.Comp.NeedFullState ||
                            args.FromTick == GameTick.Zero ||
                            args.FromTick < _timing.CurTick -
                            (uint)(TimeSpan.FromSeconds(30).Seconds * _timing.TickRate);

        if (sendFullState)
        {
            // Send complete state
            var fullData = new Dictionary<Vector2i, List<NetEntity>>(ent.Comp.FootPrintsByTile.Count);
            foreach (var (tile, footprints) in ent.Comp.FootPrintsByTile)
            {
                // Deep copy the list to avoid reference sharing
                fullData[tile] = new List<NetEntity>(footprints);
            }

            args.State = new GridFootPrintsComponentState(
                fullState: true,
                modified: fullData,
                removed: null,
                totalFootPrints: ent.Comp.TotalFootPrints
            );

            // Clear dirty tracking after full state send
            ent.Comp.DirtyTiles.Clear();
            ent.Comp.RemovedTiles.Clear();
            ent.Comp.NeedFullState = false;
        }
        else
        {
            // Send delta state - only changed tiles
            Dictionary<Vector2i, List<NetEntity>>? modifiedTiles = null;
            HashSet<Vector2i>? removedTiles = null;

            if (ent.Comp.DirtyTiles.Count > 0)
            {
                modifiedTiles = new Dictionary<Vector2i, List<NetEntity>>(ent.Comp.DirtyTiles.Count);
                foreach (var tile in ent.Comp.DirtyTiles)
                {
                    if (ent.Comp.FootPrintsByTile.TryGetValue(tile, out var footprints))
                    {
                        // Deep copy the list
                        modifiedTiles[tile] = new List<NetEntity>(footprints);
                    }
                }
            }

            if (ent.Comp.RemovedTiles.Count > 0)
            {
                removedTiles = new HashSet<Vector2i>(ent.Comp.RemovedTiles);
            }

            // Only create state if there are actual changes
            if (modifiedTiles != null || removedTiles != null)
            {
                args.State = new GridFootPrintsComponentState(
                    fullState: false,
                    modified: modifiedTiles,
                    removed: removedTiles,
                    totalFootPrints: ent.Comp.TotalFootPrints
                );

                // Clear dirty tracking after delta send
                ent.Comp.DirtyTiles.Clear();
                ent.Comp.RemovedTiles.Clear();
            }
            // If no changes, args.State stays null and nothing is sent
        }
    }

    private void OnGridFootPrintsHandleState(Entity<GridFootPrintsComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not GridFootPrintsComponentState state)
            return;

        if (state.FullState)
        {
            // Full state sync - replace entire dictionary
            ent.Comp.FootPrintsByTile.Clear();

            if (state.Modified != null)
            {
                foreach (var (tile, footprints) in state.Modified)
                {
                    ent.Comp.FootPrintsByTile[tile] = new List<NetEntity>(footprints);
                }
            }
        }
        else
        {
            // Delta state - apply changes

            // Apply removed tiles
            if (state.Removed != null)
            {
                foreach (var tile in state.Removed)
                {
                    ent.Comp.FootPrintsByTile.Remove(tile);
                }
            }

            // Apply modified/added tiles
            if (state.Modified != null)
            {
                foreach (var (tile, footprints) in state.Modified)
                {
                    ent.Comp.FootPrintsByTile[tile] = new List<NetEntity>(footprints);
                }
            }
        }

        // Always update total count
        ent.Comp.TotalFootPrints = state.TotalFootPrints;

        // Clear any local dirty tracking on client
        ent.Comp.DirtyTiles.Clear();
        ent.Comp.RemovedTiles.Clear();
    }
}
