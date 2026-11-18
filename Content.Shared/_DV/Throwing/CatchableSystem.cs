using Content.Shared._DV.Throwing;
using Content.Shared.IdentityManagement;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Player;

namespace Content.Shared.Throwing;

public sealed partial class CatchableSystem
{
    private void OnGotHit(Entity<CatcherStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ThrowHitByEvent> args)
    {
        if (!ent.Comp.Catchable.Contains(args.Args.Thrown))
            return;

        if (TryCatch(args.Args.Target, args.Args.Thrown, ent.Comp.CatchRate))
            args.Args.Handled = true;
    }

    private bool TryCatch(EntityUid catcher, EntityUid caught, float catchChance)
    {
        if (!_handsQuery.TryGetComponent(catcher, out var handsComp))
            return false; // don't do anything for walls etc

        var attemptEv = new CatchAttemptEvent(caught, catchChance);
        RaiseLocalEvent(catcher, ref attemptEv);

        if (attemptEv.Cancelled)
            return false;

        if (!SharedRandomExtensions.PredictedProb(_timing, catchChance, GetNetEntity(caught)))
            return false;

        // Try to catch!
        if (!_hands.TryPickupAnyHand(catcher, caught, handsComp: handsComp, animate: false))
            return false; // The hands are full!

        // Success!

        // We picked it up already but we still have to raise the throwing stop (but not the landing) events at the right time,
        // otherwise it will raise the events for that later while still in your hand
        if (TryComp<ThrownItemComponent>(caught, out var thrown))
            _thrown.StopThrow(caught, thrown);

        // Collisions don't work properly with PopupPredicted or PlayPredicted.
        // So we make this server only.
        if (_net.IsClient)
        {
            var selfMessage = Loc.GetString("catchable-component-success-self", ("item", caught), ("catcher", Identity.Entity(catcher, EntityManager)));
            var othersMessage = Loc.GetString("catchable-component-success-others", ("item", caught), ("catcher", Identity.Entity(catcher, EntityManager)));
            _popup.PopupEntity(selfMessage, catcher, catcher);
            _popup.PopupEntity(othersMessage, catcher, Filter.PvsExcept(catcher), true);
        }

        return true;
    }
}
