using Content.Server.Chat.Managers;
using Content.Server.DoAfter;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.StationEvents.NextEvent;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Abilities.Psionics;

public sealed class PrecognitionPowerSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

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
            psionic.ActivePowers.Remove(component);
    }

    private void OnPowerUsed(EntityUid uid, PrecognitionPowerComponent component, PrecognitionPowerActionEvent args)
    {
        var ev = new PrecognitionDoAfterEvent(_gameTiming.CurTime);
        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseDelay, ev, uid)
        {
            BreakOnDamage = true
        };

        // A custom shader for seeing visions would be nice but this will do for now.
        _statusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(uid, "TemporaryBlindness", component.UseDelay, true);
        _statusEffects.TryAddStatusEffect<SlowedDownComponent>(uid, "SlowedDown", component.UseDelay, true);

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
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            // Need to clean up the applied effects in case of cancel and alert the player.
            _statusEffects.TryRemoveStatusEffect(uid, "TemporaryBlindness");
            _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");
            _popups.PopupEntity(Loc.GetString("psionic-power-precognition-failure-by-damage"), uid, uid, PopupType.MediumCaution);
            return;
        }

        // Determines the window that will be looked at for events, avoiding events that are too close or too far to be useful.
        var minDetectWindow = TimeSpan.FromSeconds(30);
        var maxDetectWindow = TimeSpan.FromMinutes(5);
        string? message = null;

        if (!_mind.TryGetMind(uid, out _, out var mindComponent) || mindComponent.Session == null)
            return;

        if (!TryFindEarliestNextEvent(minDetectWindow, maxDetectWindow, out var nextEvent)) // A special message given if there is no event within the time window.
            message = "psionic-power-precognition-no-event-result-message";

        if (nextEvent != null && nextEvent.NextEventId != null)
            message = GetResultMessage(nextEvent.NextEventId);

        if (_random.Prob(component.RandomResultChance)) // This will replace the proper result message with a random one occasionaly to simulate some unreliablity.
            message = GetRandomResult();

        if (string.IsNullOrEmpty(message)) // If there is no message to send don't bother trying to send it.
            return;

        // Send a message describing the vision they see
        message = Loc.GetString(message);
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
    /// Gets the precognition result message corosponding to the passed event id.
    /// </summary>
    /// <returns>message string corosponding to the event id passed</returns>
    private string GetResultMessage(EntProtoId? eventId)
    {
        foreach(var (eventProto, precognitionResult) in AllPrecognitionResults())
            if (eventProto.ID == eventId && precognitionResult != null)
                return precognitionResult.Message;
        Log.Warning("Prototype " + eventId + "does not have an associated precognitionResult!");
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
                return proto.ID;
        }

        Log.Error("Result was not found after weighted pick process!");
        return null;
    }

    /// <summary>
    /// Gets the soonest nextEvent to occur within the window.
    /// </summary>
    /// <param name="minDetectWindow"></param> The earliest reletive time that will be return a nextEvent
    /// <param name="maxDetectWindow"></param> The latest reletive latest time that will be return a nextEvent
    /// <param name="earliestNextEvent"></param> The next nextEvent to occur within the window
    /// <returns>True if event was found in timeframe false otherwise.</returns>
    private bool TryFindEarliestNextEvent(TimeSpan minDetectWindow, TimeSpan maxDetectWindow, out NextEventComponent? earliestNextEvent)
    {
        TimeSpan? earliestNextEventTime = null;
        earliestNextEvent = null;
        var query = EntityQueryEnumerator<NextEventComponent>();
        while (query.MoveNext(out _, out var nextEventComponent))
        {
            // Update if the event is the most recent event that isnt too close or too far from happening to be of use
            if (nextEventComponent.NextEventTime > _gameTicker.RoundDuration() + minDetectWindow
                && nextEventComponent.NextEventTime < _gameTicker.RoundDuration() + maxDetectWindow
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
