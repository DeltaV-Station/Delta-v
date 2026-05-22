using Content.Shared.Actions;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Xenoarchaeology.Artifact.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Actions.Components;

namespace Content.Shared.Xenoarchaeology.Artifact.Components;

/// <summary>
/// This is used for handling interactions with artifacts as well as
/// storing data about artifact node graphs.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedXenoArtifactSystem)), AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenoArtifactComponent : Component
{
    public static string NodeContainerId = "node-container";

    /// <summary>
    /// Marker, if nodes graph should be generated for artifact.
    /// </summary>
    [DataField]
    public bool IsGenerationRequired = true;

    /// <summary>
    /// Container for artifact graph node entities.
    /// </summary>
    [ViewVariables]
    public Container NodeContainer = default!;

    /// <summary>
    /// The nodes in this artifact that are currently "active."
    /// This is cached and updated when nodes are removed, added, or unlocked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<NetEntity> CachedActiveNodes = new();

    /// <summary>
    /// Cache of interconnected node chunks - segments.
    /// This is cached and updated when nodes are removed, added, or unlocked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<List<NetEntity>> CachedSegments = new();

    /// <summary>
    /// Marker, if true - node activations should not happen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Suppressed;

    /// <summary>
    /// A multiplier applied to the calculated point value
    /// to determine the monetary value of the artifact.
    /// </summary>
    [DataField]
    public float PriceMultiplier = 0.10f;

    #region Unlocking
    /// <summary>
    /// How long does the unlocking state last by default.
    /// </summary>
    [DataField]
    public TimeSpan UnlockStateDuration = TimeSpan.FromSeconds(6);

    /// <summary>
    /// By how much unlocking state should be prolonged for each node that was unlocked.
    /// </summary>
    [DataField]
    public TimeSpan UnlockStateIncrementPerNode = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Minimum waiting time between unlock states.
    /// </summary>
    [DataField]
    public TimeSpan UnlockStateRefractory = TimeSpan.FromSeconds(5);

    /// <summary>
    /// DeltaV - If set, will overwrite the unlocking state time remaining as soon as a node is ready to unlock (triggers are met).
    ///   (why: letting the timer run out naturally makes artifacts less dangerous, since scientists have lots of time to run away)
    /// If null, the unlock time will just run its course naturally.
    /// </summary>
    [DataField]
    public TimeSpan? UnlockCompleteDuration = TimeSpan.FromSeconds(0.7);
    
    /// <summary>
    /// When next unlock session can be triggered.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextUnlockTime;

    /// <summary>
    /// DeltaV - The end time of the last-completed XenoArtifactUnlockingComponent phase.
    /// If null, no unlocking phase has occurred yet, and the other LastUnlocking... fields are also invalid.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? LastUnlockingEndTime = null;

    /// <summary>
    /// DeltaV - The set of related nodes that were triggered during the last completed XenoArtifactUnlockingComponent phase.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<int> LastUnlockingTriggeredNodeIndexesRelated = new();

    /// <summary>
    /// DeltaV - The list of nodes that were triggered during the last completed XenoArtifactUnlockingComponent phase.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<int> LastUnlockingTriggeredNodeIndexesOrdered = new();

    /// <summary>
    /// DeltaV - Whether the last completed unlocking phase resulted in a newly-unlocked node.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool LastUnlockingSuccessful = false;
    #endregion

    // NOTE: you should not be accessing any of these values directly. Use the methods in SharedXenoArtifactSystem.Graph
    #region Graph
    /// <summary>
    /// List of all nodes currently on this artifact.
    /// Indexes are used as a lookup table for <see cref="NodeAdjacencyMatrix"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public NetEntity?[] NodeVertices = [];

    /// <summary>
    /// Adjacency matrix that stores connections between this artifact's nodes.
    /// A value of "true" denotes an directed edge from node1 to node2, where the location of the vertex is (node1, node2)
    /// A value of "false" denotes no edge.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<List<bool>> NodeAdjacencyMatrix = new();

    public int NodeAdjacencyMatrixRows => NodeAdjacencyMatrix.Count;
    public int NodeAdjacencyMatrixColumns => NodeAdjacencyMatrix.TryGetValue(0, out var value) ? value.Count : 0;
    #endregion

    #region GenerationInfo

    /// <summary>
    /// The total number of nodes that make up this artifact.
    /// </summary>
    [DataField]
    public MinMax NodeCount = new(10, 16);

    /// <summary>
    /// The amount of nodes that go in each segment.
    /// A segment is an interconnected series of nodes.
    /// </summary>
    [DataField]
    public MinMax SegmentSize = new(5, 8);

    /// <summary>
    /// For each "layer" in a segment (set of nodes with equal depth), how many will we generate?
    /// </summary>
    [DataField]
    public MinMax NodesPerSegmentLayer = new(1, 3);

    /// <summary>
    /// How man nodes can be randomly added on top of usual distribution (per layer).
    /// </summary>
    [DataField]
    public MinMax ScatterPerLayer = new(0, 2);

    /// <summary>
    /// Effects that can be used during this artifact generation.
    /// </summary>
    [DataField]
    public EntityTableSelector EffectsTable = new NestedSelector
    {
        TableId = "XenoArtifactEffectsDefaultTable"
    };

    /// <summary>
    /// Triggers that can be used during this artefact generation.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomXenoArchTriggerPrototype> TriggerWeights = "DefaultTriggers";
    #endregion

    /// <summary>
    /// Sound effect to be played when artifact node is force-activated.
    /// </summary>
    [DataField]
    public SoundSpecifier? ForceActivationSoundSpecifier = new SoundCollectionSpecifier("ArtifactForceActivation")
    {
        Params = new()
        {
            Variation = 0.1f
        }
    };

    /// <summary>
    /// Action that allows the artifact to self activate.
    /// </summary>
    [DataField]
    public EntProtoId<InstantActionComponent> SelfActivateAction = "ActionArtifactActivate";

    /// <summary>
    /// DeltaV - Probability for each node that its effect will be hidden in the analysis console until it is unlocked.
    /// </summary>
    [DataField]
    public float LockedEffectHiddenProbability = 0.1f;

    /// <summary>
    /// DeltaV - Probability for each node that its effect will give a generic hint, rather than a specific one (until it is unlocked).
    /// </summary>
    [DataField]
    public float LockedEffectVagueProbability = 0.67f;
}

/// <summary>
/// Event raised by sentient artifact to activate itself at no durability cost.
/// </summary>
public sealed partial class ArtifactSelfActivateEvent : InstantActionEvent;
