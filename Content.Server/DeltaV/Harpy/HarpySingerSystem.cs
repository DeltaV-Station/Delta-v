using Content.Server.Instruments;
using Content.Server.Speech.Components;
using Content.Server.UserInterface;
using Content.Shared.Actions;
using Content.Shared.DeltaV.Harpy;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Server.DeltaV.Harpy
{
    public sealed class HarpySingerSystem : EntitySystem
    {
        [Dependency] private readonly InstrumentSystem _instrument = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(OnSingerInstrumentMobStateChangedEvent);
            SubscribeLocalEvent<HarpySingerComponent, MobStateChangedEvent>(OnHarpySingerMobStateChangedEvent);
            SubscribeLocalEvent<GotEquippedEvent>(OnEquip);
            SubscribeLocalEvent<GotUnequippedEvent>(OnUnequip);
            SubscribeLocalEvent<HarpySingerComponent, SingerMuzzledEvent>(OnSingerMuzzledEvent);

            // This is intended to intercept the UI event and stop the MIDI UI from opening if the
            // singer is unable to sing. Thus it needs to run before the ActivatableUISystem.
            SubscribeLocalEvent<HarpySingerComponent, OpenUiActionEvent>(OnInstrumentOpen, before: new[] { typeof(ActivatableUISystem) });
        }

        private void OnEquip(GotEquippedEvent args)
        {
            // Check if an item that makes the singer mumble is equipped to their face
            // (not their pockets!). As of writing, this should just be the muzzle.
            var uid = args.Equipee;
            if (args.Slot == "mask" &&
                TryComp<ActorComponent>(uid, out var actor) &&
                TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
                accent.ReplacementPrototype == "mumble")
            {
                RaiseLocalEvent(uid, new SingerMuzzledEvent { Muzzled = true });
                if (HasComp<ActiveInstrumentComponent>(uid))
                    _instrument.ToggleInstrumentUi(uid, actor.PlayerSession);
            }
        }

        private void OnUnequip(GotUnequippedEvent args)
        {
            var uid = args.Equipee;
            if (args.Slot == "mask" &&
                TryComp<ActorComponent>(uid, out var actor) &&
                TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
                accent.ReplacementPrototype == "mumble")
            {
                RaiseLocalEvent(uid, new SingerMuzzledEvent { Muzzled = false });
                if (HasComp<ActiveInstrumentComponent>(uid))
                    _instrument.ToggleInstrumentUi(uid, actor.PlayerSession);
            }
        }

        private void OnSingerMuzzledEvent(EntityUid uid, HarpySingerComponent component, SingerMuzzledEvent args)
        {
            component.Muzzled = args.Muzzled;
            Dirty(uid, component);
        }


        private void OnSingerInstrumentMobStateChangedEvent(EntityUid uid, InstrumentComponent component, MobStateChangedEvent args)
        {
            if (HasComp<ActiveInstrumentComponent>(uid) &&
                TryComp<ActorComponent>(uid, out var actor) &&
                _mobState.IsIncapacitated(uid))
                _instrument.ToggleInstrumentUi(uid, actor.PlayerSession);
        }

        private void OnHarpySingerMobStateChangedEvent(EntityUid uid, HarpySingerComponent component, MobStateChangedEvent args)
        {
            component.Incapacitated = _mobState.IsIncapacitated(uid);
            Dirty(uid, component);
        }

        private void OnInstrumentOpen(EntityUid uid, HarpySingerComponent component, OpenUiActionEvent args)
        {
            // Intercept the event if the singer is incapacitated or muzzled to
            // prevent the MIDI UI from opening.
            args.Handled = component.Incapacitated || component.Muzzled;

            // Explain why the user can not sing. One message is enough, and
            // being incapacitated takes presedence.
            if (component.Incapacitated)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-incapacitated"), uid, uid);
            else if (component.Muzzled)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-muzzled"), uid, uid);
        }
    }
}

public sealed partial class SingerMuzzledEvent : InstantActionEvent
{
    public bool Muzzled;
}
