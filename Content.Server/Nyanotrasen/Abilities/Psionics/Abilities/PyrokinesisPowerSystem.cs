using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Mind;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly FlammableSystem _flammableSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PyrokinesisPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, PyrokinesisPowerComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.
            var action = Spawn(PyrokinesisPowerComponent.PyrokinesisActionPrototype);
            if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind) ||
                mind.OwnedEntity is not { } ownedEntity)
            {
                return;
            }
            component.PyrokinesisPowerAction = new EntityTargetActionComponent();
            _actions.AddAction(mind.OwnedEntity.Value, action, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.PyrokinesisPowerAction;
        }

        private void OnShutdown(EntityUid uid, PyrokinesisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, PyrokinesisPowerComponent.PyrokinesisActionPrototype, null);
        }

        private void OnPowerUsed(PyrokinesisPowerActionEvent args)
        {
            if (!TryComp<FlammableComponent>(args.Target, out var flammableComponent))
                return;

            flammableComponent.FireStacks += 5;
            _flammableSystem.Ignite(args.Target, args.Target);
            _popupSystem.PopupEntity(Loc.GetString("pyrokinesis-power-used", ("target", args.Target)), args.Target, Shared.Popups.PopupType.LargeCaution);

            _psionics.LogPowerUsed(args.Performer, "pyrokinesis");
            args.Handled = true;
        }
    }
}
