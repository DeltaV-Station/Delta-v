using Content.Shared._DV.Grappling.EntitySystems;
using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Grappling.Components;

/// <summary>
/// Marks this entity as a grappler.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGrapplingSystem))]
public sealed partial class GrapplerComponent : Component
{
    /// <summary>
    /// How much time is required to escape a grapple from this entity.
    /// </summary>
    [DataField]
    public TimeSpan EscapeTime = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The sound to play when the grapple action is successful on a target.
    /// </summary>
    [DataField]
    public SoundSpecifier GrappleSound = new SoundPathSpecifier("/Audio/_DV/Grappling/grapple_action.ogg");

    /// <summary>
    /// Whether this entity can move by itself while grappling.
    /// </summary>
    [DataField]
    public bool CanMoveWhileGrappling = false;

    /// <summary>
    /// Whether this entity should also lay prone when grappling.
    /// </summary>
    [DataField]
    public bool ProneOnGrapple = false;

    /// <summary>
    /// Whether grapples from this entity should disable no, a random, or all hands of the victim.
    /// </summary>
    [DataField]
    public HandDisabling HandDisabling = HandDisabling.None;

    /// <summary>
    /// What localized string is to be used for the body part in the grapple.
    /// I.e. Jaws, Hands, Claws, etc.
    /// </summary>
    [DataField(required: true)]
    public LocId GrapplingPart;

    /// <summary>
    /// The entity, if any, which this unit is grappling.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public EntityUid? ActiveVictim = null;

    /// <summary>
    /// Cooldown for grappling to apply at the moment the grapple is broken.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Time when the cooldown for the grapple will be over.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan CooldownEnd;

    /// <summary>
    /// Which alert to show when a victim is grappled.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> GrappledAlert = "Grappled";

    /// <summary>
    /// The joint ID used between the grappler and victim.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PullJointId = null;
}

/// <summary>
/// Whether hands should be disabled by the grappling.
/// </summary>
public enum HandDisabling
{
    None, // No hands to be disabled.
    SingleRandom, // A single hand is disabled at random from available hands.
    SingleActive, // The active hand is disabled.
    All, // All hands are disabled.
}
