using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Store.Conditions;

/// <summary>
/// XOR (eXclusive-OR) Buy Condition
/// A condition where that blocks buying a listing if another listing
/// specified is bought before it. Used to enforce that a traitor
/// can only buy 1 of X options.
///
/// Primarily used for choose-your-own-adventure DAGD Implants.
/// </summary>
public sealed partial class XORBuyCondition : ListingCondition
{
    /// <summary>
    ///     Listing(s) that if bought, block this purchase, if any.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<ListingPrototype>>? Listings;

    public override bool Condition(ListingConditionArgs args)
    {
        if (!args.EntityManager.TryGetComponent<StoreComponent>(args.StoreEntity, out var storeComp))
            return false;

        var allListings = storeComp.FullListingsCatalog;

        if (Listings != null)
        {
            foreach (var blacklistListing in Listings)
            {
                foreach (var listing in allListings)
                {
                    if (listing.ID == blacklistListing.Id && listing.PurchaseAmount > 0)
                        return false; // it was purchased, so return false to fail the condition
                }
            }
        }

        return true; // If no purchases were found, return true
    }
}
