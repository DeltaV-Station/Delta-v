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
            _actions.AddAction(uid, ref component.TelegnosisActionEntity, component.TelegnosisActionId );
            _actions.TryGetActionData( component.TelegnosisActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.TelegnosisActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            {
                psionic.PsionicAbility = component.TelegnosisActionEntity;
                psionic.ActivePowers.Add(component);
                psionic.PsychicFeedback.Add(component.TelegnosisFeedback);
            }
        }

        private void OnShutdown(EntityUid uid, TelegnosisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.TelegnosisActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.TelegnosisFeedback);
            }
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
