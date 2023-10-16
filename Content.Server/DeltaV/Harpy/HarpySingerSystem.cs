using Content.Server.Instruments;
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
