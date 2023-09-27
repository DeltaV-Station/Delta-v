using Content.Shared.Actions;
using Content.Shared.StatusEffect;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Mind;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class TelegnosisPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwap = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<TelegnosisPowerComponent, TelegnosisPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<TelegnosticProjectionComponent, MindRemovedMessage>(OnMindRemoved);
        }

        private void OnInit(EntityUid uid, TelegnosisPowerComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.
            var action = Spawn(TelegnosisPowerComponent.TelegnosisActionPrototype);
            if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind) ||
                mind.OwnedEntity is not { } ownedEntity)
            {
                return;
            }
            component.TelegnosisPowerAction = new InstantActionComponent();
            _actions.AddAction(mind.OwnedEntity.Value, action, null);
        }

        private void OnShutdown(EntityUid uid, TelegnosisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, TelegnosisPowerComponent.TelegnosisActionPrototype, null);
        }

        private void OnPowerUsed(EntityUid uid, TelegnosisPowerComponent component, TelegnosisPowerActionEvent args)
        {
            var projection = Spawn(component.Prototype, Transform(uid).Coordinates);
            Transform(projection).AttachToGridOrMap();
            _mindSwap.Swap(uid, projection);

            _psionics.LogPowerUsed(uid, "telegnosis");
            args.Handled = true;
        }
        private void OnMindRemoved(EntityUid uid, TelegnosticProjectionComponent component, MindRemovedMessage args)
        {
            QueueDel(uid);
        }
    }
}
