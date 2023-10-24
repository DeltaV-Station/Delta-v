using Content.Server.Instruments;
using Content.Server.Speech.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;

namespace Content.Server.DeltaV.Harpy
{
    public sealed class HarpySingerSystem : EntitySystem
    {
        [Dependency] private readonly InstrumentSystem _instrument = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(HarpyStopSinging);
            SubscribeLocalEvent<GotEquippedEvent>(OnEquip);
        }

        // Immediately close the Midi UI window if a Singer is muzzled.
        private void OnEquip(GotEquippedEvent args)
        {
            // Check if an item that makes the singer mumble is equipped to their face
            // (not their pockets!). As of writing, this should just be the muzzle.
            var uid = args.Equipee;
            if (args.Slot == "mask" &&
                HasComp<ActiveInstrumentComponent>(uid) &&
                TryComp<ActorComponent>(uid, out var actor) &&
                TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
                accent.ReplacementPrototype == "mumble")
                _instrument.ToggleInstrumentUi(uid, actor.PlayerSession);
        }

        //Immediately closes the Midi UI window if a Singer is incapacitated
        private void HarpyStopSinging(EntityUid uid, InstrumentComponent component, MobStateChangedEvent args)
        {
            if (HasComp<ActiveInstrumentComponent>(uid) && TryComp<ActorComponent>(uid, out var actor) && _mobState.IsIncapacitated(uid))
            {
                _instrument.ToggleInstrumentUi(uid, actor.PlayerSession);
            }
        }
    }
}
