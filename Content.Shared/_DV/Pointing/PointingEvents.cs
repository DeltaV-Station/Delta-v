using Robust.Shared.Map;

namespace Content.Shared.Pointing;

/// <summary>
/// Raised on the entity who is pointing after they point at at a tile.
/// </summary>
/// <param name="Pointed">MapCoordinates pointed at by the entity.</param>
[ByRefEvent]
public readonly record struct AfterPointedAtTileEvent(MapCoordinates Pointed);
