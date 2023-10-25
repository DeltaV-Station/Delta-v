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

            SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(OnSingerInstrumentMobStateChangedEvent);
            SubscribeLocalEvent<GotEquippedEvent>(OnEquip);

            // This is intended to intercept the UI event and stop the MIDI UI from opening if the
            // singer is unable to sing. Thus it needs to run before the ActivatableUISystem.
            SubscribeLocalEvent<HarpySingerComponent, OpenUiActionEvent>(OnInstrumentOpen, before: new[] { typeof(ActivatableUISystem) });
        }

        /// <summary>
        /// Close the MIDI UI when a muzzle is equipped.
        /// </summary>
        private void OnEquip(GotEquippedEvent args)
        {
            // Check if an item that makes the singer mumble is equipped to their face
            // (not their pockets!). As of writing, this should just be the muzzle.
            if (HasComp<ActiveInstrumentComponent>(args.Equipee) &&
                TryComp<ActorComponent>(args.Equipee, out var actor) &&
                TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
                accent.ReplacementPrototype == "mumble" &&
                args.Slot == "mask")
            {
                _instrument.ToggleInstrumentUi(args.Equipee, actor.PlayerSession);
            }
        }

        /// <summary>
        /// Close the MIDI UI when incapacitated.
        /// </summary>
        private void OnSingerInstrumentMobStateChangedEvent(EntityUid uid, InstrumentComponent component, MobStateChangedEvent args)
        {
            if (HasComp<ActiveInstrumentComponent>(uid) &&
                TryComp<ActorComponent>(uid, out var actor) &&
                _mobState.IsIncapacitated(uid))
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

            // Set this event as handled when the singer should be incapable of singing in order
            // to stop the ActivatableUISystem event from opening the MIDI UI.
            args.Handled = incapacitated || muzzled;

            // Explain why the user can not sing. One message is enough, and
            // being incapacitated takes presedence.
            if (incapacitated)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-incapacitated"), uid, uid);
            else if (muzzled)
                _popupSystem.PopupEntity(Loc.GetString("no-sing-while-muzzled"), uid, uid);
        }
    }
}
