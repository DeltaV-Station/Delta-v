using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Shared.StatusEffect;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Mind;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class NoosphericZapPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly BeamSystem _beam = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<NoosphericZapPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<NoosphericZapPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<NoosphericZapPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, NoosphericZapPowerComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.
            var action = Spawn(NoosphericZapPowerComponent.NoosphericZapActionPrototype);
            if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind) ||
                mind.OwnedEntity is not { } ownedEntity)
            {
                return;
            }
            component.NoosphericZapPowerAction = new EntityTargetActionComponent();
            _actions.AddAction(mind.OwnedEntity.Value, action, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.NoosphericZapPowerAction;
        }

        private void OnShutdown(EntityUid uid, NoosphericZapPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, NoosphericZapPowerComponent.NoosphericZapActionPrototype, null);
        }

        private void OnPowerUsed(NoosphericZapPowerActionEvent args)
        {
            if (!HasComp<PotentialPsionicComponent>(args.Target))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target))
                return;

            _beam.TryCreateBeam(args.Performer, args.Target, "LightningNoospheric");

            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(5), false);
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(10), false, "StutteringAccent");

            _psionics.LogPowerUsed(args.Performer, "noospheric zap");
            args.Handled = true;
        }
    }
}
