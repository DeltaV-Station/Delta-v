using System.Collections.Generic;
using Content.Shared.Chat;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._DV.Traits.Assorted;

public sealed class SlouchySystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    private readonly Dictionary<EntityUid, float> _queuedDrains = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlouchyComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<SlouchyComponent, DoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<SlouchyComponent, EntitySpokeEvent>(OnSpeak);
        SubscribeLocalEvent<SlouchyComponent, DidEquipHandEvent>(OnPickup);
        SubscribeLocalEvent<SlouchyComponent, UserInteractHandEvent>(OnInteract);

        SubscribeLocalEvent<UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<DroppedEvent>(OnDrop);
        SubscribeLocalEvent<SlouchyComponent, MeleeAttackEvent>(OnMeleeAttack);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_queuedDrains.Count == 0)
            return;

        var drains = new Dictionary<EntityUid, float>(_queuedDrains);
        _queuedDrains.Clear();

        foreach (var (uid, amount) in drains)
        {
            ApplyDrain(uid, amount);
        }
    }

    private void DrainSelf(EntityUid uid, float amount)
    {
        _queuedDrains[uid] = _queuedDrains.GetValueOrDefault(uid) + amount;
    }

    private void ApplyDrain(EntityUid uid, float amount)
    {
        if (Deleted(uid))
            return;

        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;
        _stamina.TakeStaminaDamage(uid, amount, stamina, source: null, visual: false);
    }

    // literally every events that makes the character *do* something that actually requires OOC effort :godo:
    private void OnEmote(EntityUid uid, SlouchyComponent component, ref EmoteEvent args)
    {
        DrainSelf(uid, component.EmoteStaminaDrain);
    }

    private void OnDoAfter(EntityUid uid, SlouchyComponent component, DoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        DrainSelf(uid, component.DoAfterStaminaDrain);
    }

    private void OnSpeak(EntityUid uid, SlouchyComponent component, EntitySpokeEvent args)
    {
        DrainSelf(uid, component.SpeakStaminaDrain);
    }

    private void OnPickup(EntityUid uid, SlouchyComponent component, DidEquipHandEvent args)
    {
        DrainSelf(uid, component.PickupStaminaDrain);
    }

    private void OnInteract(EntityUid uid, SlouchyComponent component, UserInteractHandEvent args)
    {
        DrainSelf(uid, component.InteractStaminaDrain);
    }

    private void OnUseInHand(UseInHandEvent args)
    {
        if (!TryComp<SlouchyComponent>(args.User, out var slouchy))
            return;

        DrainSelf(args.User, slouchy.UseInHandStaminaDrain);
    }

    private void OnDrop(DroppedEvent args)
    {
        if (!TryComp<SlouchyComponent>(args.User, out var slouchy))
            return;

        DrainSelf(args.User, slouchy.DropStaminaDrain);
    }

    private void OnMeleeAttack(EntityUid uid, SlouchyComponent component, ref MeleeAttackEvent args)
    {
        DrainSelf(uid, component.MeleeStaminaDrain);
    }
}