using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Reputation;

/// <summary>
/// Objective groups that offerings can be picked from.
/// Up to 1 objective of this group can be offered.
/// </summary>
[Prototype]
public sealed partial class ReputationOfferingGroupsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// The groups to pick from.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<WeightedRandomPrototype>> Groups = new();
}
