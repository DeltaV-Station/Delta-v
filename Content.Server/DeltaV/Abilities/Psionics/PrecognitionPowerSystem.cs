using Content.Server.Chat.Managers;
using Content.Server.DoAfter;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Actions;
using Content.Shared.DeltaV.StationEvents;
using Content.Shared.DoAfter;
using Content.Shared.Psionics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Server.StationEvents.Components;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PrecognitionPowerSystem : EntitySystem
    {
        [Dependency] public readonly GameTicker GameTicker = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IComponentFactory _factory = default!;
        [Dependency] private readonly IChatManager _chat = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PrecognitionPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PrecognitionPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PrecognitionPowerComponent, PrecognitionPowerActionEvent>(OnPowerUsed);

            SubscribeLocalEvent<PrecognitionPowerComponent, PrecognitionDoAfterEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, PrecognitionPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.PrecognitionActionEntity, component.PrecognitionActionId);
            _actions.TryGetActionData(component.PrecognitionActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.PrecognitionActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            {
                psionic.PsionicAbility = component.PrecognitionActionEntity;
                psionic.ActivePowers.Add(component);
            }
        }

        private void OnShutdown(EntityUid uid, PrecognitionPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.PrecognitionActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
            }
        }

        private void OnPowerUsed(EntityUid uid, PrecognitionPowerComponent component, PrecognitionPowerActionEvent args)
        {
            var ev = new PrecognitionDoAfterEvent(_gameTiming.CurTime);
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseDelay, ev, uid)
            {
                BreakOnDamage = true
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId);
            component.DoAfter = doAfterId;

            _psionics.LogPowerUsed(uid, "Precognition");
            args.Handled = true;
        }

        /// <summary>
        /// Upon completion will send a message to the user corrosponding to the next station event to occour.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void OnDoAfter(EntityUid uid, PrecognitionPowerComponent component, PrecognitionDoAfterEvent args)
        {
            var minDetectWindow = TimeSpan.FromSeconds(30); // Determines the window that will be looked at for events avoiding events that are too close or too far to be useful.
            var maxDetectWindow = TimeSpan.FromMinutes(5);
            string? message = null;

            if (!_mind.TryGetMind(uid, out _, out var mindComponent) || mindComponent.Session == null)
                return;

            if (!TryFindEarliestNextEvent(minDetectWindow, maxDetectWindow, out var nextEvent)) // A special message given if there is no event within the time window
                message = "psionic-power-precognition-no-event-result-message";

            if (nextEvent != null)
                message = GetResultMessage(nextEvent.NextEventId);

            if (_random.Prob(component.RandomResultChance) || true)
                message = GetRandomResult();

            if (message == string.Empty || message == null)
                return;

            // Send a message describing the vision they see
            _chat.ChatMessageToOne(Shared.Chat.ChatChannel.Server,
                    message,
                    Loc.GetString("chat-manager-server-wrap-message", ("message", message)),
                    uid,
                    false,
                    mindComponent.Session.Channel,
                    Color.PaleVioletRed);

            component.DoAfter = null;
        }

        /// <summary>
        /// Sets "message" to the localized message of the precognitionResult that that matches "nextEventId">
        /// </summary>
        /// <returns>true if a corosponding precognitionResult was found false otherwise</returns>
        private string GetResultMessage(EntProtoId nextEventId)
        {
            foreach(var (eventProto, precognitionResult) in AllPrecognitionResults())
                if (eventProto.ID == nextEventId && precognitionResult != null)
                {
                    return Loc.GetString(precognitionResult.Message);
                }
            Log.Warning("Prototype " + nextEventId + "does not have an associated precognitionResult!");
            return string.Empty;
        }

        /// <summary>
        /// </summary>
        /// <returns>The localized string of a weighted randomly chosen precognition result</returns>
        public string? GetRandomResult()
        {
            var precognitionResults = AllPrecognitionResults();
            var sumOfWeights = 0;
            foreach (var precognitionResult in precognitionResults.Values)
                sumOfWeights += (int)precognitionResult.Weight;

            sumOfWeights = _random.Next(sumOfWeights);
            foreach (var (proto, stationEvent) in precognitionResults)
            {
                sumOfWeights -= (int)stationEvent.Weight;

                if (sumOfWeights <= 0)
                    return Loc.GetString(proto.ID);
            }

            Log.Error("Result was not found after weighted pick process!");
            return null;
        }

        /// <summary>
        /// Gets the soonest nextEvent to occur within the window.
        /// </summary>
        /// <param name="minDetectWindow"></param> The earliest reletive time that will be return a nextEvent
        /// <param name="maxDetectWindow"></param> The latest reletive latest time that will be return a nextEvent
        /// <param name="earliestNextEvent"></param> the next nextEvent to occur within the window
        /// <returns></returns>
        private bool TryFindEarliestNextEvent(TimeSpan minDetectWindow, TimeSpan maxDetectWindow, out NextEventComponent? earliestNextEvent)
        {
            TimeSpan? earliestNextEventTime = null;
            earliestNextEvent = null;
            var query = EntityQueryEnumerator<NextEventComponent>();
            while (query.MoveNext(out var uid, out var nextEventComponent))
            {
                // Update if the event is the most recent event that isnt too close or too far from happening to be of use
                if (nextEventComponent.NextEventTime > GameTicker.RoundDuration() + minDetectWindow
                    && nextEventComponent.NextEventTime < GameTicker.RoundDuration() + maxDetectWindow
                    && earliestNextEvent == null
                    || nextEventComponent.NextEventTime < earliestNextEventTime)
                    earliestNextEvent ??= nextEventComponent;
            }
            if (earliestNextEvent == null)
                return false;
            return true;
        }

        public Dictionary<EntityPrototype, PrecognitionResultComponent> AllPrecognitionResults()
        {
            var allEvents = new Dictionary<EntityPrototype, PrecognitionResultComponent>();
            foreach (var prototype in _prototype.EnumeratePrototypes<EntityPrototype>())
            {
                if (prototype.Abstract)
                    continue;

                if (!prototype.TryGetComponent<PrecognitionResultComponent>(out var precognitionResult, _factory))
                    continue;

                allEvents.Add(prototype, precognitionResult);
            }

            return allEvents;
        }
    }
}
