using Content.Shared.CombatMode;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared._DV.Throwing; // Delta-V - Telekinesis
using Content.Shared.StatusEffectNew; // Delta-V - Telekinesis

namespace Content.Shared.Throwing;

public sealed partial class CatchableSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ThrownItemSystem _thrown = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<CombatModeComponent> _combatModeQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CatchableComponent, ThrowDoHitEvent>(OnDoHit);
        SubscribeLocalEvent<CatcherStatusEffectComponent, StatusEffectRelayedEvent<ThrowHitByEvent>>(OnGotHit); // Delta-V - Telekinesis

        _handsQuery = GetEntityQuery<HandsComponent>();
        _combatModeQuery = GetEntityQuery<CombatModeComponent>();
    }

    private void OnDoHit(Entity<CatchableComponent> ent, ref ThrowDoHitEvent args)
    {
        // Is the catcher in combat mode if required?
        if (ent.Comp.RequireCombatMode && (!_combatModeQuery.TryComp(args.Target, out var combatModeComp) || !combatModeComp.IsInCombatMode))
            return;
        // Is the catcher able to catch this item?
        if (!_whitelist.IsWhitelistPassOrNull(ent.Comp.CatcherWhitelist, args.Target))
            return;

        if (!TryCatch(args.Target, ent, ent.Comp.CatchChance)) // Delta-V - Telekinesis
            return;

        _audio.PlayPvs(ent.Comp.CatchSuccessSound, ent.Owner);
        args.Handled = true;
    }

    private void OnGotHit(Entity<CatcherStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ThrowHitByEvent> args)  // Delta-V - Telekinesis
    {
        if (!ent.Comp.Catchable.Contains(args.Args.Thrown))
            return;

        if (TryCatch(args.Args.Target, args.Args.Thrown, ent.Comp.CatchRate))
            args.Args.Handled = true;
    }

    private bool TryCatch(EntityUid catcher, EntityUid caught, float catchChance) // Delta-V - Telekinesis - Isolated this into a function
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
            return true;

        var selfMessage = Loc.GetString("catchable-component-success-self", ("item", caught), ("catcher", Identity.Entity(catcher, EntityManager)));
        var othersMessage = Loc.GetString("catchable-component-success-others", ("item", caught), ("catcher", Identity.Entity(catcher, EntityManager)));
        _popup.PopupEntity(selfMessage, catcher, catcher);
        _popup.PopupEntity(othersMessage, catcher, Filter.PvsExcept(catcher), true);
        return true;
    }

    private bool CheckCatching(EntityUid catcher, EntityUid caught, float catchChance)
    {
        if (!_handsQuery.TryGetComponent(catcher, out var handsComp) || !_hands.CanPickupAnyHand(catcher, caught, handsComp: handsComp))
            return false; // don't do anything for walls etc

        float chance;
        // TODO see about being able to cancel the embeddable event instead of this
        // Make it so that I can import the chances and such instead of having this here. Now that I've made it only used here it works.
        // being a catcher takes priority since most things aren't catchable by default.
        // I should probably just make it take the higher catch rate, though... hm.
        // if (_status.TryEffectsWithComp<CatcherStatusEffectComponent>(catcher, out var statuses))
        // {
        //     chance = 0;
        //     foreach (var status in statuses)
        //     {
        //         chance = chance > status.Comp1.CatchRate ? chance : status.Comp1.CatchRate;
        //         if (!status.Comp1.Catchable.Contains(caught))
        //             return false;
        //     }
        // }
        // else if (TryComp<CatchableComponent>(caught, out var catchable))
        // {
        //     chance = catchable.CatchChance;
        //     // Is the catcher in combat mode if required?
        //     if (catchable.RequireCombatMode && (!_combatModeQuery.TryComp(catcher, out var combatModeComp) ||
        //                                         !combatModeComp.IsInCombatMode))
        //         return false;
        //     // Is the catcher able to catch this item?
        //     if (!_whitelist.IsWhitelistPassOrNull(catchable.CatcherWhitelist, catcher))
        //         return false;
        // }
        // else
        //     return false;

        // needs the comps
        var attemptEv = new CatchAttemptEvent(caught, catchChance);
        RaiseLocalEvent(catcher, ref attemptEv);

        if (attemptEv.Cancelled)
            return false;

        if (!SharedRandomExtensions.PredictedProb(_timing, catchChance, GetNetEntity(caught)))
            return false;

        return true;
    }
}
