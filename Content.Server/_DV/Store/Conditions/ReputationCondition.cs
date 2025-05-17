using Content.Shared._DV.Reputation;
using Content.Shared.Store;

namespace Content.Server._DV.Store.Conditions;

/// <summary>
/// Requires that an uplink using <see cref="ContractsComponent"/> has enough reputation.
/// This is ignored for nukie uplinks and surplus crates.
/// </summary>
public sealed partial class ReputationCondition : ListingCondition
{
    /// <summary>
    /// The required reputation for traitors.
    /// This is unused for nukie uplinks.
    /// </summary>
    [DataField(required: true)]
    public int Reputation;

    public override bool Condition(ListingConditionArgs args)
    {
        if (!args.EntityManager.TryGetComponent<StoreContractsComponent>(args.StoreEntity, out var store))
            return true; // nukie uplink or a surplus

        var reputation = args.EntityManager.System<ReputationSystem>();
        if (reputation.GetMindReputation(store.Mind) is not {} rep)
            return false; // uplink implant in non-traitor, no epic goodies allowed

        return rep >= Reputation;
    }
}
