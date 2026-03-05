using Content.Server._DV.StationEvents.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.StationEvents.Components;

/// <summary>
/// Spawns a small amount of randomly-colored foxfires,
/// centered around either the Oracle or Sophic Grammateus.
/// </summary>
[RegisterComponent, Access(typeof(GlimmerFoxfireSpawnRule))]
public sealed partial class GlimmerFoxfireSpawnRuleComponent : Component
{
    /// <summary>
    /// Minimum+ amounts of foxfires to spawn.
    /// </summary>
    [DataField]
    public int MinimumSpawned = 10;

    /// <summary>
    /// Maximum amounts of foxfires to spawn.
    /// </summary>
    [DataField]
    public int MaximumSpawned = 20;

    /// <summary>
    /// Maximum distance from the Oracle or Sophic Grammateus in which the
    /// foxfire will be spawned.
    /// </summary>
    [DataField]
    public int SpawnRange = 10;

    /// <summary>
    /// Prototype to be spawned.
    /// </summary>
    [DataField]
    public EntProtoId FoxfirePrototype = "Foxfire";

    /// <summary>
    /// Available colors for the foxfire's light.
    /// </summary>
    [DataField]
    public List<Color>? RandomColorList = new();
}
