using Content.Server.Chat.Managers;
using Content.Server.DoAfter;
using Content.Server._DV.StationEvents.NextEvent;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Player;

namespace Content.Server.Abilities.Psionics;

public sealed class PrecognitionPowerSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// A map between game rule prototypes and their results to give.
    /// </summary>
    public Dictionary<EntProtoId, PrecognitionResultComponent> Results = new();

    public override void Initialize()
    {
        base.Initialize();
        CachePrecognitionResults();

        SubscribeLocalEvent<PrecognitionPowerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PrecognitionPowerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PrecognitionPowerComponent, PrecognitionPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<PrecognitionPowerComponent, PrecognitionDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    private void OnMapInit(Entity<PrecognitionPowerComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.PrecognitionActionEntity, ent.Comp.PrecognitionActionId);
        _actions.StartUseDelay(ent.Comp.PrecognitionActionEntity);
        if (TryComp<PsionicComponent>(ent, out var psionic) && psionic.PsionicAbility == null)
        {
            psionic.PsionicAbility = ent.Comp.PrecognitionActionEntity;
            psionic.ActivePowers.Add(ent.Comp);
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
        var ev = new PrecognitionDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseDelay, ev, uid)
        {
            BreakOnDamage = true
        };

        // A custom shader for seeing visions would be nice but this will do for now.
        _statusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(uid, "TemporaryBlindness", component.UseDelay, true);
        _statusEffects.TryAddStatusEffect<SlowedDownComponent>(uid, "SlowedDown", component.UseDelay, true);

        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        component.DoAfter = doAfterId;

        var player = _audio.PlayGlobal(component.VisionSound, Filter.Entities(uid), true);
        if (player != null)
            component.SoundStream = player.Value.Entity;
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
            component.SoundStream = _audio.Stop(component.SoundStream);
            _statusEffects.TryRemoveStatusEffect(uid, "TemporaryBlindness");
            _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");

            _popup.PopupEntity(
                Loc.GetString("psionic-power-precognition-failure-by-damage"),
                uid,
                uid,
                PopupType.SmallCaution);

            if (_actions.TryGetActionData(component.PrecognitionActionEntity, out var actionData))
                // If canceled give a short delay before being able to try again
                actionData.Cooldown =
                    (_gameTicker.RoundDuration(),
                    _gameTicker.RoundDuration() + TimeSpan.FromSeconds(15));
            return;
        }

        // Determines the window that will be looked at for events, avoiding events that are too close or too far to be useful.
        var minDetectWindow = TimeSpan.FromSeconds(30);
        var maxDetectWindow = TimeSpan.FromMinutes(10);

        if (!_mind.TryGetMind(uid, out _, out var mindComponent) || mindComponent.Session == null)
            return;

        var nextEvent = FindEarliestNextEvent(minDetectWindow, maxDetectWindow);
        LocId? message = nextEvent?.NextEventId is {} nextEventId
            ? GetResultMessage(nextEventId)
            // A special message given if there is no event within the time window.
            : "psionic-power-precognition-no-event-result-message";

        if (_random.Prob(component.RandomResultChance)) // This will replace the proper result message with a random one occasionaly to simulate some unreliablity.
            message = GetRandomResult();

        if (message is not {} locId) // If there is no message to send don't bother trying to send it.
            return;

        // Send a message describing the vision they see
        var msg = Loc.GetString(locId);
        _chat.ChatMessageToOne(ChatChannel.Server,
            msg,
            Loc.GetString("chat-manager-server-wrap-message", ("message", msg)),
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
    private LocId? GetResultMessage(EntProtoId eventId)
    {
        if (!Results.TryGetValue(eventId, out var result))
        {
            Log.Error($"Prototype {eventId} does not have an associated precognitionResult!");
            return null;
        }

        return result.Message;
    }

    /// <summary>
    /// </summary>
    /// <returns>The locale message id of a weighted randomly chosen precognition result</returns>
    public LocId? GetRandomResult()
    {
        // funny weighted random
        var sumOfWeights = 0f;
        foreach (var precognitionResult in Results.Values)
            sumOfWeights += precognitionResult.Weight;

        sumOfWeights = (float) _random.Next((double) sumOfWeights);
        foreach (var precognitionResult in Results.Values)
        {
            sumOfWeights -= precognitionResult.Weight;

            if (sumOfWeights <= 0f)
                return precognitionResult.Message;
        }

        Log.Error("Precognition result was not found after weighted pick process!");
        return null;
    }

    /// <summary>
    /// Gets the soonest nextEvent to occur within the window.
    /// </summary>
    /// <param name="minDetectWindow"></param> The earliest reletive time that will be return a nextEvent
    /// <param name="maxDetectWindow"></param> The latest reletive latest time that will be return a nextEvent
    /// <returns>Component for the next event to occour if one exists in the window.</returns>
    private NextEventComponent? FindEarliestNextEvent(TimeSpan minDetectWindow, TimeSpan maxDetectWindow)
    {
        TimeSpan? earliestNextEventTime = null;
        NextEventComponent? earliestNextEvent = null;
        var query = EntityQueryEnumerator<NextEventComponent>();
        while (query.MoveNext(out var nextEventComponent))
        {
            // Update if the event is the most recent event that isnt too close or too far from happening to be of use
            if (nextEventComponent.NextEventTime > _gameTicker.RoundDuration() + minDetectWindow
                && nextEventComponent.NextEventTime < _gameTicker.RoundDuration() + maxDetectWindow
                && earliestNextEvent == null
                || nextEventComponent.NextEventTime < earliestNextEventTime)
            {
                earliestNextEvent ??= nextEventComponent;
            }
        }
        return earliestNextEvent;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>())
            return;

        CachePrecognitionResults();
    }

    private void CachePrecognitionResults()
    {
        Results.Clear();
        foreach (var prototype in _proto.EnumeratePrototypes<EntityPrototype>())
        {
            if (prototype.Abstract)
                continue;

            if (!prototype.TryGetComponent<PrecognitionResultComponent>(out var precognitionResult, _factory))
                continue;

            Results.Add(prototype.ID, precognitionResult);
        }
    }
}
