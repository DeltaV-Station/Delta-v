using Content.Server._DV.Psionics.Systems;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Chat.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.GameRules;

/// <summary>
/// Forces a mind swap on all non-insulated potential psionic entities.
/// </summary>
internal sealed class MassMindSwapRule : StationEventSystem<MassMindSwapRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PsionicSystem _psionic = default!;

    private TimeSpan _warningSoundLength;
    private ResolvedSoundSpecifier _resolvedWarningSound = String.Empty;

    protected override void Started(EntityUid uid, MassMindSwapRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _resolvedWarningSound = _audio.ResolveSound(component.SwapWarningSound);
        _warningSoundLength = _audio.GetAudioLength(_resolvedWarningSound);

        component.SwapTime = Timing.CurTime + component.Delay;
        component.SoundTime = component.SwapTime - _warningSoundLength;

        var announcement = Loc.GetString(component.AnnouncementText, ("time", component.Delay.TotalSeconds));
        var sender = Loc.GetString(component.AnnouncementSender);
        _chat.DispatchGlobalAnnouncement(announcement, sender, true, component.AnnouncementSound, Color.White);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MassMindSwapRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var comp, out var ruleComp))
        {
            if (comp.SoundTime != null && comp.SoundTime <= Timing.CurTime)
            {
                _audio.PlayGlobal(_resolvedWarningSound, Filter.Broadcast(), true);
                comp.SoundTime = null;
                continue;
            }

            if (comp.SwapTime == null || comp.SwapTime > Timing.CurTime)
                continue;

            SwapMinds(comp);
            comp.SwapTime = null;
            GameTicker.EndGameRule(uid, ruleComp);
        }
    }

    private void SwapMinds(MassMindSwapRuleComponent component)
    {
        List<EntityUid> psionicPool = new();
        List<EntityUid> psionicActors = new();

        var query = EntityQueryEnumerator<PotentialPsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out var mobState))
        {
            if (!_mobStateSystem.IsAlive(psion, mobState) || !_psionic.CanBeTargeted(psion))
                continue;

            if (HasComp<ActorComponent>(psion))
            {
                psionicActors.Add(psion);
                psionicPool.Add(psion);
            }
            else if (!component.OnlyPlayers)
                psionicPool.Add(psion);
        }

        var maxPairs = component.MaxNumberOfPairs;
        if (maxPairs.HasValue)
            _random.Next(1, maxPairs.Value);

        foreach (var actor in psionicActors)
        {
            while (psionicPool.Count > 0 && maxPairs is null or > 0)
            {
                var other = _random.PickAndTake(psionicPool);
                // Don't be yourself. Find someone else.
                if (other == actor)
                    continue;

                // A valid swap target has been found.
                // Remove this actor from the pool of swap candidates before they go.
                psionicPool.Remove(actor);
                if (maxPairs.HasValue)
                    maxPairs--;

                _psionic.SwapMinds(actor, other, false, component.IsTemporary, component.IgnoreMindshields);
                break;
            }
        }
    }
}
