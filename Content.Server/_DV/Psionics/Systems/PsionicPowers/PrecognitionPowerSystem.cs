using Content.Server._DV.StationEvents.NextEvent;
using Content.Server.Chat.Managers;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This system lets a psionic user foretell the next event with some inconsistency.
/// </summary>
public sealed class PrecognitionPowerSystem : SharedPrecognitionPowerSystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <summary>
    /// A map between game rule prototypes and their results to give.
    /// </summary>
    public Dictionary<EntProtoId, PrecognitionResultComponent> Results = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrecognitionPowerComponent, PrecognitionDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        CachePrecognitionResults();
    }

    /// <summary>
    /// Send a message to the user about the next station event.
    /// </summary>
    /// <param name="psionic">The source of the psionic usage.</param>
    /// <param name="args">The doAfter event.</param>
    private void OnDoAfter(Entity<PrecognitionPowerComponent> psionic, ref PrecognitionDoAfterEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        psionic.Comp.RemoveSavedDoAfterId();
        Dirty(psionic);

        if (args.Cancelled)
        {
            // Need to clean up the applied effects in case of cancel and alert the player.
            // TODO: Port over the TemporaryBlindness effect to the new StatusEffectSystem.
            // When Upstream ports it over, replace this with it.
            psionic.Comp.SoundStream = Audio.Stop(psionic.Comp.SoundStream);
            StatusEffects.TryRemoveStatusEffect(args.User, "TemporaryBlindness");
            Movement.TryUpdateMovementSpeedModDuration(args.User, PrecognitionSlowdown, TimeSpan.Zero, 0.5f);

            Popup.PopupEntity(
                Loc.GetString("psionic-power-precognition-failure-by-damage"),
                args.User,
                args.User,
                PopupType.SmallCaution);

            if (Action.GetAction(psionic.Comp.ActionEntity) is {} actionData)
                Action.SetCooldown(actionData.Owner, _timing.CurTime, _timing.CurTime + psionic.Comp.CancellationCooldown);
            return;
        }

        // Determines the window that will be looked at for events, avoiding events that are too close or too far to be useful.

        if (!_player.TryGetSessionByEntity(args.User, out var session))
            return;

        var nextEvent = FindEarliestNextEvent(psionic.Comp.MinEventTimeDistance, psionic.Comp.MaxEventTimeDistance);

        LocId? message = nextEvent?.NextEventId is {} nextEventId
            ? GetResultMessage(nextEventId)
            // A special message given if there is no event within the time window.
            : "psionic-power-precognition-no-event-result-message";

        if (_random.Prob(psionic.Comp.RandomResultChance)) // This will replace the proper result message with a random one occasionally to simulate some unreliability.
            message = GetRandomResult();

        if (message is not {} locId) // If there is no message to send, don't bother trying to send it.
            return;

        // Send a message describing the vision they see
        var msg = Loc.GetString(locId);
        _chat.ChatMessageToOne(ChatChannel.Server,
            msg,
            Loc.GetString("chat-manager-server-wrap-message", ("message", msg)),
            psionic,
            false,
            session.Channel,
            Color.PaleVioletRed);
    }

    /// <summary>
    /// Gets the precognition result message corosponding to the passed event id.
    /// </summary>
    /// <returns>message string corosponding to the event id passed</returns>
    private LocId? GetResultMessage(EntProtoId eventId)
    {
        if (Results.TryGetValue(eventId, out var result))
            return result.Message;

        Log.Error($"Prototype {eventId} does not have an associated precognitionResult!");
        return null;

    }

    /// <summary>
    /// </summary>
    /// <returns>The locale message id of a weighted randomly chosen precognition result</returns>
    public LocId? GetRandomResult()
    {
        // funny weighted random
        var sumOfWeights = 0f;
        foreach (var precognitionResult in Results.Values)
        {
            sumOfWeights += precognitionResult.Weight;
        }

        sumOfWeights = (float) _random.Next(sumOfWeights);
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

        var windowStart = _timing.CurTime + minDetectWindow;
        var windowEnd = _timing.CurTime + maxDetectWindow;

        while (query.MoveNext(out var nextEventComponent))
        {
            // Update if the event is the most recent event that isn't too close or too far from happening to be of use
            if (windowStart < nextEventComponent.NextEventTime && nextEventComponent.NextEventTime < windowEnd
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
        foreach (var prototype in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (prototype.Abstract
                || !prototype.TryGetComponent<PrecognitionResultComponent>(out var precognitionResult, _factory))
                continue;

            Results.Add(prototype.ID, precognitionResult);
        }
    }
}
