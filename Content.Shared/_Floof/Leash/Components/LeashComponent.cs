using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Floofstation.Leash.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LeashComponent : Component
{
    /// <summary>
    ///     Maximum number of leash joints that this entity can create.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxJoints = 1;

    /// <summary>
    ///     Default length of the leash joint.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Length = 3.5f;

    /// <summary>
    ///     List of possible lengths this leash may be assigned to be the user. If null, the length cannot be changed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float[]? LengthConfigs;

    /// <summary>
    ///     Maximum distance between the anchor and the puller beyond which the leash will break.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxDistance = 8f;

    /// <summary>
    ///     The time it takes for one entity to attach/detach the leash to/from another entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan AttachDelay = TimeSpan.FromSeconds(2f), DetachDelay = TimeSpan.FromSeconds(2f);

    /// <summary>
    ///     The time it takes for the leashed entity to detach itself from this leash.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan SelfDetachDelay = TimeSpan.FromSeconds(8f);

    [DataField, AutoNetworkedField]
    public SpriteSpecifier? LeashSprite;

    [DataField]
    public TimeSpan NextPull = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan PullInterval = TimeSpan.FromSeconds(1.5f);

    /// <summary>
    ///     List of all joints and their respective pulled entities created by this leash.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<LeashData> Leashed = new();

    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class LeashData
    {
        /// <summary>
        ///     Id of the joint created by this leash. May be null if this leash does not currently create a joint
        ///     (e.g. because it's attached to the same entity who holds it)
        /// </summary>
        [DataField]
        public string? JointId = null;

        [DataField]
        public NetEntity Pulled = NetEntity.Invalid;

        /// <summary>
        ///     Entity used to visualize the leash. Created dynamically.
        /// </summary>
        [DataField]
        public NetEntity? LeashVisuals = null;

        public LeashData(string? jointId, NetEntity pulled)
        {
            JointId = jointId;
            Pulled = pulled;
        }
    };
}
