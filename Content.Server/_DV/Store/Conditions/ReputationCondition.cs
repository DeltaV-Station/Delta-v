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
        var reputation = args.EntityManager.System<ReputationSystem>();
        if (args.StoreEntity is not {} pda || reputation.GetReputation(pda) is not {} rep)
            return true; // nukie uplink or a surplus

        return rep >= Reputation;
    }
}
