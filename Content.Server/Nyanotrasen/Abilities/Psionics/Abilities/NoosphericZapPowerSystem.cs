using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Shared.StatusEffect;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Content.Server.Inventory;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Mind;
using Content.Shared.Actions.Events;
using Content.Shared.Electrocution;
using Content.Shared.Lightning;

namespace Content.Server.Abilities.Psionics
{
    public sealed class NoosphericZapPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ServerInventorySystem _inventory = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedLightningSystem _lightning = default!;
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
            _actions.AddAction(uid, ref component.NoosphericZapActionEntity, component.NoosphericZapActionId );
            _actions.TryGetActionData( component.NoosphericZapActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.NoosphericZapActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            {
                psionic.PsionicAbility = component.NoosphericZapActionEntity;
                psionic.ActivePowers.Add(component);
            }
        }

        private void OnShutdown(EntityUid uid, NoosphericZapPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.NoosphericZapActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
            }
        }

        private void OnPowerUsed(NoosphericZapPowerActionEvent args)
        {
            _psionics.LogPowerUsed(args.Performer, "noospheric zap");

            _beam.TryCreateBeam(args.Performer, args.Target, "LightningNoospheric");

            if (!HasComp<PotentialPsionicComponent>(args.Target))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target))
                return;

            if (_inventory.TryGetSlotEntity(args.Target, "gloves", out var gloves)
                && TryComp<InsulatedComponent>(gloves, out var insulation)
                && insulation.Coefficient == 0)
                return;

            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(5), false);
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(10), false, "StutteringAccent");

            args.Handled = true;
        }
    }
}
