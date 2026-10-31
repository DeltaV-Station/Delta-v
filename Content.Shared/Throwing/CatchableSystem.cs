using Content.Shared._DV.Throwing;
using Content.Shared.CombatMode;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffectNew;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.Throwing;

public sealed partial class CatchableSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly ThrownItemSystem _thrown = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<CombatModeComponent> _combatModeQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CatchableComponent, ThrowDoHitEvent>(OnDoHit);
        SubscribeLocalEvent<CatcherStatusEffectComponent, StatusEffectRelayedEvent<ThrowHitByEvent>>(OnGotHit);

        _handsQuery = GetEntityQuery<HandsComponent>();
        _combatModeQuery = GetEntityQuery<CombatModeComponent>();
    }

    private void OnDoHit(Entity<CatchableComponent> ent, ref ThrowDoHitEvent args)
    {
        if (CheckCatching(args.Target, ent, out var hand))
        {
            TryCatch(args.Target, ent, ent.Comp.CatchChance, hand);
            _audio.PlayPvs(ent.Comp.CatchSuccessSound, ent.Owner);
            args.Handled = true;
        }

    }

    private void OnGotHit(Entity<CatcherStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ThrowHitByEvent> args)
    {
        if (CheckCatching(args.Args.Target, args.Args.Thrown, out var hand))
        {
            TryCatch(args.Args.Target, args.Args.Thrown, ent.Comp.CatchRate, hand);
            args.Args.Handled = true;
        }
    }

    private void TryCatch(EntityUid catcher, EntityUid caught, float chance, HandsComponent? hands)
    {
        // Try to catch!
        if (!_hands.TryPickupAnyHand(catcher, caught, handsComp: hands, animate: false) || hands == null)
            return; // The hands are full!

        // Success!

        // We picked it up already but we still have to raise the throwing stop (but not the landing) events at the right time,
        // otherwise it will raise the events for that later while still in your hand
        if (TryComp<ThrownItemComponent>(caught, out var thrown))
            _thrown.StopThrow(caught, thrown);

        // Collisions don't work properly with PopupPredicted or PlayPredicted.
        // So we make this server only.
        if (_net.IsClient)
            return;

        var selfMessage = Loc.GetString("catchable-component-success-self",
            ("item", caught),
            ("catcher", Identity.Entity(catcher, EntityManager)));
        var othersMessage = Loc.GetString("catchable-component-success-others",
            ("item", caught),
            ("catcher", Identity.Entity(catcher, EntityManager)));
        _popup.PopupEntity(selfMessage, catcher, catcher);
        _popup.PopupEntity(othersMessage, catcher, Filter.PvsExcept(catcher), true);
    }

    public bool CheckCatching(EntityUid catcher, EntityUid caught, out HandsComponent? hand)
    {
        if (!_handsQuery.TryGetComponent(catcher, out var handsComp) || !_hands.CanPickupAnyHand(catcher, caught, handsComp: handsComp))
        {
            hand = null;
            return false; // don't do anything for walls etc
        }

        hand = handsComp;

        float chance;
        // TODO see about being able to cancel the embeddable event instead of this
        // being a catcher takes priority since most things aren't catchable by default.
        // I should probably just make it take the higher catch rate, though... hm.
        if (_status.TryEffectsWithComp<CatcherStatusEffectComponent>(catcher, out var statuses))
        {
            chance = 0;
            foreach (var status in statuses)
            {
                chance = chance > status.Comp1.CatchRate ? chance : status.Comp1.CatchRate;
                if (!status.Comp1.Catchable.Contains(caught))
                    return false;
            }
        }
        else if (TryComp<CatchableComponent>(caught, out var catchable))
        {
            chance = catchable.CatchChance;
            // Is the catcher in combat mode if required?
            if (catchable.RequireCombatMode && (!_combatModeQuery.TryComp(catcher, out var combatModeComp) ||
                                                !combatModeComp.IsInCombatMode))
                return false;
            // Is the catcher able to catch this item?
            if (!_whitelist.IsWhitelistPassOrNull(catchable.CatcherWhitelist, catcher))
                return false;
        }
        else
            return false;

        // needs the comps
        var attemptEv = new CatchAttemptEvent(caught, chance);
        RaiseLocalEvent(catcher, ref attemptEv);

        if (attemptEv.Cancelled)
            return false;

        if (!SharedRandomExtensions.PredictedProb(_timing, chance, GetNetEntity(caught)))
            return false;

        hand = handsComp;
        return true;
    }
}
