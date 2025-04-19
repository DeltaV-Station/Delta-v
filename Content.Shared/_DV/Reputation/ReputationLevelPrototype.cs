using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Reputation;

/// <summary>
/// Data associated with a reputation level.
/// </summary>
[Prototype]
public sealed partial class ReputationLevelPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// Name of the reputation level to display in UIs.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Minimum reputation someone needs to get this.
    /// </summary>
    [DataField(required: true)]
    public int Reputation;

    /// <summary>
    /// Maximum number of contracts that can be active at once.
    /// </summary>
    [DataField(required: true)]
    public int MaxContracts;

    /// <summary>
    /// Maximum number of offering slots that there can be.
    /// </summary>
    [DataField(required: true)]
    public int MaxOfferings;

    /// <summary>
    /// Maximum difficulty for objectives that can be rolled.
    /// <c>ReputationCondition</c> should be used for fine-grained control.
    /// </summary>
    [DataField]
    public float MaxDifficulty = 6f;

    /// <summary>
    /// Offering groups that can be used.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> OfferingGroups = "ReputationOfferings";
}
