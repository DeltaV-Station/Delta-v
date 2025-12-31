using Content.Shared.Hands.Components;
using Content.Shared.Stunnable;

namespace Content.Shared.Hands.EntitySystems;

/// <summary>
/// This is for events that don't affect normal hand functions but do care about hands.
/// </summary>
public abstract partial class SharedHandsSystem
{
    private void InitializeEventListeners()
    {
        SubscribeLocalEvent<HandsComponent, GetStandUpTimeEvent>(OnStandupArgs);
        SubscribeLocalEvent<HandsComponent, KnockedDownRefreshEvent>(OnKnockedDownRefresh);
    }

    /// <summary>
    /// Reduces the time it takes to stand up based on the number of hands we have available.
    /// </summary>
    private void OnStandupArgs(Entity<HandsComponent> ent, ref GetStandUpTimeEvent time)
    {
        if (!HasComp<KnockedDownComponent>(ent))
            return;

        var hands = GetEmptyHandCount(ent.Owner);

        if (hands == 0)
            return;

        time.DoAfterTime *= (float)ent.Comp.Count / (hands + ent.Comp.Count);
    }

    private void OnKnockedDownRefresh(Entity<HandsComponent> ent, ref KnockedDownRefreshEvent args)
    {
        var freeHands = CountFreeHands(ent.AsNullable());
        var totalHands = GetHandCount(ent.AsNullable());

        // Can't crawl around without any hands.
        // Entities without the HandsComponent will always have full crawling speed.
        if (totalHands == 0)
            args.SpeedModifier = 0f;
        // DeltaV - 1 hand free = 75% movespeed, no hands free = 50% movespeed
        else if (totalHands > freeHands)
        {
            // For some reason, this feels a bit dirty
            // 0 free hands out of 1 total hands = (1-(1-0)*.50) = 50% additional reduction to current move speed mod
            // 1 free hand out of 2 total hands = (1-(2-1)*.25) = 75%
            // 1 free hand out of 4 total hands = (1-(4-1)*.125) = 62.5% 
            // 6 free hands out of 10 total hands hands = (1-(10-6)*0.05) = 80% 
            var reductionPerHand = 1f / (totalHands + totalHands);
            args.SpeedModifier *= (float)Math.Clamp(1f - (totalHands - freeHands) * reductionPerHand, 0.5, 1.0);
        }
        // END DeltaV
    }
}
