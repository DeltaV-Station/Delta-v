using Content.Server.Mobs;
using Content.Server.Instruments;
using Content.Shared.Instruments;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.DeltaV.Harpy;

namespace Content.Server.DeltaV.Harpy
{
    public class HarpySingerSystem : EntitySystem
    {
        [Dependency] private readonly InstrumentSystem _instrument = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(OnRemoveInstrumentAbility);
            SubscribeLocalEvent<HarpySingerComponent, MobStateChangedEvent>(OnAddInstrumentAbility);
        }
        //Immediately stops a harpy from singing if they're critted or killed, and prevents them from starting a new song
        private void OnRemoveInstrumentAbility(EntityUid uid, InstrumentComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState != MobState.Dead || args.NewMobState != MobState.Critical)
            {
                RemComp<InstrumentComponent>(uid);
                return;
            }
        }
        //Allows a harpy to sing again after being revived.
        private void OnAddInstrumentAbility(EntityUid uid, HarpySingerComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState != MobState.Alive)
                return;

            var instrument = EnsureComp<InstrumentComponent>(uid);
            EnsureComp<InstrumentComponent>(uid);
           _instrument.SetInstrumentProgram(instrument, 52, 0);
        }
    }
}

