using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
namespace Content.Shared._DV.Vision.Components;

/// <summary>
/// This is used for granting components to mobs when they observe the owning entity.
/// </summary>
[RegisterComponent][NetworkedComponent][AutoGenerateComponentState(fieldDeltas: true)][AutoGenerateComponentPause]
public sealed partial class GrantComponentsOnObservationComponent : Component
{
    /// <summary>
    /// The entities that have already been affected by this.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)][AutoNetworkedField]
    public HashSet<EntityUid> AffectedEntities = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))][AutoPausedField][AutoNetworkedField]
    public TimeSpan? NextGrantAttempt;

    /// <summary>
    /// The interval at which observers will be checked.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The range in which entities will be affected.
    /// </summary>
    [DataField][AutoNetworkedField]
    public float Range = 20f;

    /// <summary>
    /// If true, this will affect entities with silicon.
    /// </summary>
    [DataField]
    public bool AffectSilicons;

    /// <summary>
    /// If true, this will affect entities with blindness.
    /// </summary>
    [DataField]
    public bool AffectBlinded;

    /// <summary>
    /// If true, this will affect entities inside containers.
    /// </summary>
    [DataField]
    public bool AffectInContainers;

    /// <summary>
    /// If true, this will affect the owner of the component.
    /// </summary>
    [DataField]
    public bool AffectSelf;

    /// <summary>
    /// The components to grant.
    /// </summary>
    [DataField]
    public ComponentRegistry? Grant;

    /// <summary>
    /// If true, existing components will be overwritten with the granted ones.
    /// </summary>
    [DataField]
    public bool RemoveExisting;

    /// <summary>
    /// The sound played for the observer receiving the components.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundObserver;

    /// <summary>
    /// The sound played for the observer from the position of the owner.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundOwner;

    /// <summary>
    /// The popup message to display to the observer when the components are granted.
    /// </summary>
    [DataField]
    public string? Message;

    /// <summary>
    /// The size of the popup message displayed to the observer when the components are granted.
    /// </summary>
    [DataField]
    public PopupType VisualType = PopupType.Small;
}

/// <summary>
/// An observing mob is about to be granted components by a <see cref="GrantComponentsOnObservationComponent"/>. If this event is canceled, the components will not be granted.
/// </summary>
/// <param name="source">The <see cref="EntityUid"/> of the <see cref="GrantComponentsOnObservationComponent"/> owner.</param>
/// <param name="target">The <see cref="EntityUid"/> of the observing mob.</param>
public sealed class ObserverGrantedComponents(EntityUid source, EntityUid target) : CancellableEntityEventArgs
{
    /// <summary>
    /// The <see cref="EntityUid"/> of the <see cref="GrantComponentsOnObservationComponent"/> owner.
    /// </summary>
    public EntityUid? Source = source;

    /// <summary>
    /// The <see cref="EntityUid"/> of the observing mob.
    /// </summary>
    public EntityUid? Target = target;
}
