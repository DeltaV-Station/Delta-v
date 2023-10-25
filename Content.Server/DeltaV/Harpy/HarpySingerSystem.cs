using Content.Server.Instruments;
using Content.Server.Speech.Components;
using Content.Server.UserInterface;
using Content.Shared.DeltaV.Harpy;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Content.Shared.Zombies;

namespace Content.Server.DeltaV.Harpy
{
    public sealed class HarpySingerSystem : EntitySystem
    {
        [Dependency] private readonly InstrumentSystem _instrument = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
            SubscribeLocalEvent<GotEquippedEvent>(OnEquip);
            SubscribeLocalEvent<EntityZombifiedEvent>(OnZombified);

            // This is intended to intercept the UI event and stop the MIDI UI from opening if the
            // singer is unable to sing. Thus it needs to run before the ActivatableUISystem.
            SubscribeLocalEvent<HarpySingerComponent, OpenUiActionEvent>(OnInstrumentOpen, before: new[] { typeof(ActivatableUISystem) });
        }

        private void OnEquip(GotEquippedEvent args)
        {
            // Check if an item that makes the singer mumble is equipped to their face
            // (not their pockets!). As of writing, this should just be the muzzle.
            if (TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
                accent.ReplacementPrototype == "mumble" &&
                args.Slot == "mask")
            {
                CloseMidiUi(args.Equipee);
            }
        }

        private void OnMobStateChangedEvent(EntityUid uid, InstrumentComponent component, MobStateChangedEvent args)
        {
            if (_mobState.IsIncapacitated(uid))
                CloseMidiUi(args.Target);
        }

        private void OnZombified(ref EntityZombifiedEvent args)
        {
            CloseMidiUi(args.Target);
        }

        /// <summary>
        /// Closes the MIDI UI if it is open.
        /// </summary>
        private void CloseMidiUi(EntityUid uid)
        {
            if (HasComp<ActiveInstrumentComponent>(uid) &&
                TryComp<ActorComponent>(uid, out var actor))
            {
                _instrument.ToggleInstrumentUi(uid, actor.PlayerSession);
            }
        }

        /// <summary>
        /// Prevent the player from opening the MIDI UI under some circumstances.
        /// </summary>
        private void OnInstrumentOpen(EntityUid uid, HarpySingerComponent component, OpenUiActionEvent args)
        {
            var muzzled = _inventorySystem.TryGetSlotEntity(uid, "mask", out var maskUid) &&
                TryComp<AddAccentClothingComponent>(maskUid, out var accent) &&
                accent.ReplacementPrototype == "mumble";

            var incapacitated = _mobState.IsIncapacitated(uid);

            var zombified = TryComp<ZombieComponent>(uid, out var _);

            // Set this event as handled when the singer should be incapable of singing in order
            // to stop the ActivatableUISystem event from opening the MIDI UI.
            args.Handled = incapacitated || muzzled || zombified;

            // Explain why the user can not sing. One message is enough, and
            // being incapacitated takes presedence.
            if (incapacitated)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-incapacitated"), uid, uid, PopupType.Medium);
            else if (zombified)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-zombified"), uid, uid, PopupType.Medium);
            else if (muzzled)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-muzzled"), uid, uid, PopupType.Medium);
        }
    }
}
