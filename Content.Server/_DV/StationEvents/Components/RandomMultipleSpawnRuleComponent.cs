using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._DV.StationEvents.Components;

/// <summary>
/// Spawns a random amount of entities at a random tile on a station using TryGetRandomTile.
/// </summary>
[RegisterComponent, Access(typeof(RandomSpawnRule))]
public sealed partial class RandomMultipleSpawnRuleComponent : Component
{
    /// <summary>
    /// The entity to be spawned.
    /// </summary>
    [DataField("prototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = string.Empty;

    /// <summary>
    /// The minimum amount of entities to be spawned.
    /// </summary>
    [DataField]
    public int MinAmount = 1;

    /// <summary>
    /// The maximum amount of entities to be spawned.
    /// </summary>
    [DataField]
    public int MaxAmount = 1;
}
