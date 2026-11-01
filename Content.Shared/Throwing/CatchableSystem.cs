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
    [Dependency] private readonly StatusEffectsSystem _status = default!; // Delta-V - Telekinesis
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
        if (CheckCatching(args.Target, ent, ent.Comp.CatchChance))
        {
            TryCatch(args.Target, ent); // Delta-V - Telekinesis
            _audio.PlayPvs(ent.Comp.CatchSuccessSound, ent.Owner);
            args.Handled = true;
        }

    }

    private void OnGotHit(Entity<CatcherStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ThrowHitByEvent> args)  // Delta-V - Telekinesis
    {
        if (CheckCatching(args.Args.Target, args.Args.Thrown, ent.Comp.CatchRate) && (ent.Comp.Catchable.Contains(args.Args.Thrown) || ent.Comp.Catchable.Count == 0))
        {
            TryCatch(args.Args.Target, args.Args.Thrown);
            args.Args.Handled = true;
        }
    }

    private void TryCatch(EntityUid catcher, EntityUid caught) // Delta-V - Telekinesis - Isolated this into a function
    {
        // Try to catch!
        if (!_handsQuery.TryComp(catcher, out var handComp) || !_hands.TryPickupAnyHand(catcher, caught, handsComp: handComp, animate: false))
            return; // The hands are full! (Or don't exist.)

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

    private bool CheckCatching(EntityUid catcher, EntityUid caught, float catchChance)
    {
        if (!_handsQuery.TryGetComponent(catcher, out var handsComp) || !_hands.CanPickupAnyHand(catcher, caught, handsComp: handsComp))
            return false; // don't do anything for walls etc

        float chance;
        // TODO see about being able to cancel the embeddable event instead of this
        // Make it so that I can import the chances and such instead of having this here. Now that I've made it only used here it works.
        // being a catcher takes priority since most things aren't catchable by default.
        // I should probably just make it take the higher catch rate, though... hm.

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
