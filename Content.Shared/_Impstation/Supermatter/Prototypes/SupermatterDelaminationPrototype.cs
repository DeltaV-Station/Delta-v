using Content.Shared.Atmos;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._Impstation.Supermatter.Prototypes;

/// <summary>
/// This is a prototype for Supermatter Delaminations
/// </summary>
[Prototype]
public sealed class SupermatterDelaminationPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<SupermatterDelaminationPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    /// <summary>
    /// The requirements for this delamination to occur. All non-null requirements must be met for this delamination to be valid.
    /// </summary>
    [DataField]
    public SupermatterDelaminationRequirements Requirements;

    /// <summary>
    /// The GameRules to be added after delamination.
    /// </summary>
    [DataField][AlwaysPushInheritance]
    public EntProtoId[] GameRules = [];
    
    /// <summary>
    /// The effects that will be applied to the supermatter entity when this delamination occurs.
    /// </summary>
    [DataField][AlwaysPushInheritance]
    public EntityEffect[] SupermatterEffects = [];
    
    /// <summary>
    /// The effects that will be applied to mobs on the same map as the supermatter entity when this delamination occurs.
    /// </summary>
    [DataField][AlwaysPushInheritance]
    public EntityEffect[] MobEffects = [];

    [DataField]
    public string? Message;
}

[DataDefinition, Serializable, NetSerializable]
public readonly partial record struct SupermatterDelaminationRequirements
{
    /// <summary>
    /// The minimum power required for this delamination to occur. If null, there is no minimum.
    /// </summary>
    [DataField]
    public float? MinPower { get; init; }
    
    /// <summary>
    /// The maximum power required for this delamination to occur. If null, there is no maximum.
    /// </summary>
    [DataField]
    public float? MaxPower { get; init; }
    
    /// <summary>
    /// The minimum moles of absorbed gas required for this delamination to occur. If null, there is no minimum.
    /// </summary>
    [DataField]
    public float? MinMoles { get; init; }
    
    /// <summary>
    /// The maximum moles of absorbed gas required for this delamination to occur. If null, there is no maximum.
    /// </summary>
    [DataField]
    public float? MaxMoles { get; init; }
    
    /// <summary>
    /// The minimum glimmer required for this delamination to occur. If null, there is no minimum.
    /// </summary>
    [DataField]
    public float? MinGlimmer { get; init; }
    
    /// <summary>
    /// The maximum glimmer required for this delamination to occur. If null, there is no maximum.
    /// </summary>
    [DataField]
    public float? MaxGlimmer { get; init; }
    
    /// <summary>
    /// The minimum moles of each gas required for this delamination to occur. If null, there is no minimum.
    /// </summary>
    [DataField]
    public Dictionary<Gas, float>? GasMoles { get; init; }
}