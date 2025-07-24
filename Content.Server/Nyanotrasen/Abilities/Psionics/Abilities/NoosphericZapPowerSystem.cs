using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Shared.StatusEffect;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Content.Server.Inventory;
using Content.Server.Lightning;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Mind;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class NoosphericZapPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly LightningSystem _lightning = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;

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
            
            if (HasComp<PotentialPsionicComponent>(args.Target))
                return;

            if (!HasComp<PsionicInsulationComponent>(args.Target))
                return;

            _lightning.ShootLightning(args.Performer, args.Target);

            args.Handled = true;
        }
    }
}
