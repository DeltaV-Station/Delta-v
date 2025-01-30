using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Content.Shared.EntityEffects;

namespace Content.Shared._DV.Ailments;

/// <summary>
///     Represents some condition on the body that isn't numerical damage
/// </summary>
[Prototype]
public sealed partial class AilmentPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    [ViewVariables]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     What this ailment shows up as on the health scanner, if anything
    /// </summary>
    [DataField]
    public LocId? HealthAnalyzerDescription { get; set; } = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public string? LocalizedHealthAnalyzerDescription => HealthAnalyzerDescription is null ? null : Loc.GetString(HealthAnalyzerDescription);

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<AilmentPrototype>))]
    [ViewVariables]
    public string[]? Parents { get; private set; } = default!;

    [NeverPushInheritance]
    [AbstractDataField]
    [ViewVariables]
    public bool Abstract { get; private set; } = default!;

    /// <summary>
    ///     Entity effects to run when this ailment becomes active
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables]
    public EntityEffect[] Mount { get; private set; } = default!;

    /// <summary>
    ///     Entity effects to run when this ailment becomes inactive
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables]
    public EntityEffect[] Unmount { get; private set; } = default!;

    /// <summary>
    ///     Entity effects to run twice per second when this ailment is active
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables]
    public EntityEffect[] Periodic { get; private set; } = default!;
}

/// <summary>
///     Represents a transition from one ailment to another, e.g. from being fine to having a minor bone fracture
/// </summary>
[Prototype]
public sealed partial class AilmentTransitionPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    [ViewVariables]
    public string ID { get; private set; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<AilmentTransitionPrototype>))]
    [ViewVariables]
    public string[]? Parents { get; private set; } = default!;

    [NeverPushInheritance]
    [AbstractDataField]
    [ViewVariables]
    public bool Abstract { get; private set; } = default!;

    /// <summary>
    ///     The chance for the transition to trigger if the trigger conditions are met
    /// </summary>
    [ViewVariables]
    public float TriggerChance { get; private set; } = 1f;

    /// <summary>
    ///     Conditions for the transition, that when met, will cause the transition to fire without outside intervention
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables]
    public EntityEffectCondition[]? Triggers { get; private set; } = default!;

    /// <summary>
    ///     Conditions for the transition, that are required but don't cause a transition without outside intervention
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables]
    public EntityEffectCondition[] Conditions { get; private set; } = new EntityEffectCondition[] {};

    /// <summary>
    ///     Effects that run when this transition is taken
    /// </summary>
    [DataField(serverOnly: true)]
    [ViewVariables]
    public EntityEffect[]? Effects { get; private set; } = default!;

    /// <summary>
    ///     Which ailment we're transitioning from
    /// </summary>
    [DataField(required: true)]
    [ViewVariables]
    public ProtoId<AilmentPrototype>? Start { get; private set; } = default!;

    /// <summary>
    ///     Which ailment we're transitioning to
    /// </summary>
    [DataField(required: true)]
    [ViewVariables]
    public ProtoId<AilmentPrototype>? End { get; private set; } = default!;
}

/// <summary>
///     A collection of ailments and transitions between them. Only one ailment in a pack can be active at a time.
/// </summary>
[Prototype]
public sealed partial class AilmentPackPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    [ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField]
    [ViewVariables]
    public ProtoId<AilmentPrototype>[] Ailments { get; private set; } = default!;

    [DataField]
    [ViewVariables]
    public ProtoId<AilmentTransitionPrototype>[] Transitions { get; private set; } = default!;
}
