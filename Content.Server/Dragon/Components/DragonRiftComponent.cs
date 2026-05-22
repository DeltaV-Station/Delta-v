using Content.Shared.Dragon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Dragon;

[RegisterComponent]
public sealed partial class DragonRiftComponent : SharedDragonRiftComponent
{
    /// <summary>
    /// Dragon that spawned this rift.
    /// </summary>
    [DataField("dragon")] public EntityUid? Dragon;

    /// <summary>
    /// How long the rift has been active.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("accumulator")]
    public float Accumulator = 0f;

    /// <summary>
    /// The maximum amount we can accumulate before becoming impervious.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxAccumuluator")] public float MaxAccumulator = 300f;

    /// <summary>
    /// Accumulation of the spawn timer.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("spawnAccumulator")]
    public float SpawnAccumulator = 30f;

    /// <summary>
    /// How long it takes for a new spawn to be added.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("spawnCooldown")]
    public float SpawnCooldown = 30f;

    [ViewVariables(VVAccess.ReadWrite), DataField("spawn", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string SpawnPrototype = "MobCarpDragon";

    //Begin DeltaV - Elite spawns on dragon rifts

    [DataField]
    public bool SpawnElites = true;

    [DataField("spawnElite")]
    public EntProtoId? SpawnElitePrototype = "MobSharkminnowDragon";

    /// <summary>
    /// Every N-th spawn is the elite where N is Elite frequency
    /// </summary>
    [DataField]
    public int SpawnEliteFrequency = 5;

    /// <summary>
    /// Accumulation of elite spawns
    /// Starts at the same value as Frequency to guarantee the first spawn to be elite
    /// </summary>
    [DataField]
    public int SpawnEliteAccumulator = 5;
    //End DeltaV - Elite spawns on dragon rifts
}
