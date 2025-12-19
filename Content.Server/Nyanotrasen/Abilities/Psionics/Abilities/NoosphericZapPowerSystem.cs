using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Server.Lightning;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class NoosphericZapPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly LightningSystem _lightning = default!;

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

            if (_actions.GetAction(component.NoosphericZapActionEntity) is not { Comp.UseDelay: not null })
            {
                _actions.StartUseDelay(component.NoosphericZapActionEntity);
            }

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
            if (!HasComp<PotentialPsionicComponent>(args.Target))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target))
                return;

            _lightning.ShootLightning(args.Performer, args.Target);

            _psionics.LogPowerUsed(args.Performer, "noospheric zap");
            args.Handled = true;
        }
    }
}
